# Templating Loops

TeaPie can expand a single request block into multiple independent requests, driven by a collection. This solves data-driven scenarios (seeding N entities, running the same check against a list of inputs) without copy-pasting request blocks or writing imperative loops in `.csx` scripts.

| | |
| --- | --- |
| **Syntax** | `{% for <item> in <source> %}` ... `{% endfor %}` |
| **Where** | `.http` request files and `.tp` files (in their `--- HTTP` section) |
| **Purpose** | Repeats the enclosed request block once per item in `<source>`, producing one independent request per iteration. |

## How It Fits the Pipeline

Loop expansion runs **after** the pre-request (`-init.csx`) script and **before** requests are split into individual test steps. This means:

- Any variable set in `-init.csx` via `tp.SetVariable(...)` is already available as a loop source.
- Each resulting request behaves exactly like a normal request: it gets its own name, directives (`## TEST-...`, `## AUTH-PROVIDER`, retry directives), and its own entry in the test report.
- A file is treated as a template only if it contains at least one supported tag: `{% for %}`, `{% if %}`, `{% unless %}`, `{% assign %}` or `{% raw %}` (whitespace control such as `{%-` is allowed). A file using only `{% if %}`/`{% assign %}` with no loop at all is still parsed and rendered by Fluid (see [Conditions and Assignments](#conditions-and-assignments)).
- Any other file is left completely unchanged, so templating has no effect on ordinary request files. That includes a file whose body happens to contain `{%` text, such as a strftime format (`"{%Y-%m-%d}"`) or a Jinja payload. TeaPie logs a warning for such a file, in case a tag name was misspelled (tag names are case-sensitive).
- In a file that **is** a template, every `{%` is read as Fluid syntax. Wrap literal `{%` text in `{% raw %}...{% endraw %}`, for example `{"fmt": "{% raw %}{%Y-%m-%d}{% endraw %}"}`. Otherwise the file fails to parse. The same rule applies to `#` and `//` comment lines, because `# @name` lines must be templated too. A commented-out loop such as `# {% for i in (1..2) %}` therefore still runs.

## Collection Sources

Three kinds of collections can be looped over:

### 1. A Variable

The most common case: a collection set in the pre-request script.

```csharp
// 001-loop-over-partners-init.csx
tp.SetVariable("Partners", new[]
{
    new { Name = "Acme Corp", RegistrationId = "01245" },
    new { Name = "Globex Inc", RegistrationId = "012426" },
    new { Name = "Initech", RegistrationId = "012427" }
});
```

```http
{% for partner in Partners %}
### Create partner {{ forloop.index }}: {{ partner.Name }}
# @name CreatePartner{{ forloop.index }}
## TEST-EXPECT-STATUS: [201]
POST {{ApiBaseUrl}}/partners
Content-Type: application/json

{ "name": "{{ partner.Name }}", "registrationId": "{{ partner.RegistrationId }}" }
{% endfor %}
```

Any public, readable property of the item can be accessed with `{{ item.PropertyName }}`. Property names are **case-sensitive** — `{{ partner.Name }}` works, `{{ partner.name }}` resolves to an empty value.

Dotted variable names (e.g. a variable stored as `Temp.FreePartners`) work the same way: `{% for partner in Temp.FreePartners %}`.

### 2. An Inline Literal List

Useful for quick smoke tests without a pre-request script:

```http
{% for status in ("new", "used", "certified") %}
### Create listing #{{ forloop.index }} with status: {{ status }}
# @name CreateListing{{ forloop.index }}
## TEST-EXPECT-STATUS: [201]
POST {{ApiBaseUrl}}/listings
Content-Type: application/json

{ "title": "Listing {{ status }}" }
{% endfor %}
```

Supported literal types inside the parentheses, comma-separated:

- Double-quoted strings: `"new"`, `"used"`
- Booleans: `true`, `false`
- Numbers (integer or decimal): `1`, `42`, `1.5`

An empty list `()` (or whitespace-only, `(   )`) resolves to zero items, which is treated as an error (see [Guards and Error Handling](#guards-and-error-handling)).

### 3. A Numeric Range

For a fixed repeat count with no real data behind it:

```http
{% for i in (1..5) %}
### Create item {{ i }}
# @name CreateItem{{ i }}
## TEST-EXPECT-STATUS: [201]
POST {{ApiBaseUrl}}/items
Content-Type: application/json

{ "title": "Item {{ i }}", "index": {{ i }} }
{% endfor %}
```

`(1..5)` is inclusive on both ends and produces 5 iterations.

## Loop Options

A collection can be followed by options, separated by spaces, in any order:

| Option | Meaning |
| --- | --- |
| `limit: n` | Iterate over at most `n` items (standard Liquid) |
| `offset: n` | Skip the first `n` items (standard Liquid) |
| `reversed` | Iterate in reverse order, applied after `offset`/`limit` (standard Liquid) |

```http
{% for partner in Partners offset: 1 limit: 10 reversed %}
...
{% endfor %}
```

The value of `limit:`/`offset:` can be a number or a variable (`limit: MaxPartners`). A literal `limit:` also lowers the count used by the 1000-request guard below; a variable one does not, since its value is not known before rendering.

A loop source does **not** accept filters — `{% for x in Items | sort %}` is an error, because Liquid's `for` tag only takes a collection followed by the options above.

## The `forloop` Object

Inside a loop body, `{{ forloop.index }}` gives the current **1-based** iteration number. It is commonly used to build unique request names (`# @name CreateItem{{ forloop.index }}`) and unique values inside the request body.

The fields you'll use most often:

| Field | Meaning |
| --- | --- |
| `forloop.index` | 1-based iteration number |
| `forloop.index0` | 0-based iteration number |
| `forloop.first` | `true` on the first iteration, `false` otherwise |
| `forloop.last` | `true` on the last iteration, `false` otherwise |

All four are usable anywhere inside the loop body, including inside `# @name` declarations. `forloop` is Fluid's native loop object, so its other standard fields (`forloop.length`, `forloop.rindex`, `forloop.rindex0`) are also available, even though TeaPie doesn't add anything on top of them.
Expansion happens **before** request names are parsed, so `# @name Create{{ forloop.index }}`
becomes a literal name like `# @name Create1` before TeaPie's own request parser ever sees it —
the parser and everything downstream of it (including `tp.Responses[...]` lookups and the test
report) only ever see the rendered literal name.

## How Expansion Works (and What It Leaves Alone)

Loop expansion only touches `{{ }}` expressions that reference the loop variable (e.g. `partner`, `i`, `status`) or `forloop`. Every other `{{ }}` expression — TeaPie variables, functions, or named-request references — is left **untouched**, to be resolved later by TeaPie's own variable/function resolution:

```http
Input (inside the loop):
POST {{ApiBaseUrl}}/partners/{{ partner.RegistrationId }}
X-Trace-Id: {{$guid}}

After expansion (partner.RegistrationId = "01245"):
POST {{ApiBaseUrl}}/partners/01245
X-Trace-Id: {{$guid}}
```

`{{ApiBaseUrl}}` and `{{$guid}}` are resolved normally, after expansion, exactly as in a request file without any loop.

### Advanced: Referencing an Expanded Request by Name

Because request names differ per iteration (`CreatePartner1`, `CreatePartner2`, ...), a later request cannot reference a fixed name like `{{CreatePartner.response...}}`. Combine `forloop.index` with the `prepend`/`append` filters to build the reference dynamically:

```http
{% for tenant in Tenants %}
### Create company ({{ tenant.Label }})
## TEST-JSON-HAS-ID-PROPERTY: {{ forloop.index | prepend: "Temp.Attachments.CompanyId_" }}
POST {{ApiBaseUrl}}/companies

### Set license for company ({{ tenant.Label }})
POST {{ApiBaseUrl}}/companies/{{ forloop.index | prepend: "{{Temp.Attachments.CompanyId_" | append: "}}" }}/licenses
{% endfor %}
```

This pattern stores each company's id under `Temp.Attachments.CompanyId_1`, `Temp.Attachments.CompanyId_2`, etc. (for example via a custom `TEST-JSON-HAS-ID-PROPERTY` directive in a post-response script), and rebuilds `{{Temp.Attachments.CompanyId_1}}`-style references on the fly for the next request in the same iteration.

## Naming Requests Inside a Loop

If you name a request with `# @name CreatePartner`, **every** iteration produces a request with that same literal name. TeaPie logs a warning for this (naming the file, the duplicated name, and how many requests share it) but does not fail the run — only the last-registered request remains resolvable by that name. This tolerance applies only to templated files (see [How It Fits the Pipeline](#how-it-fits-the-pipeline)). In a file without templates, a duplicate `# @name` remains an error, as before. Always include `{{ forloop.index }}` (or another per-iteration value) in the name to keep it unique:

```http
# @name CreatePartner{{ forloop.index }}
```

The same warning fires for duplicate names arising between a plain request and a loop-produced one, or across multiple loops in the same file — not just within a single loop.

## Multiple Loops in One File

A single request file can contain several `{% for %}` loops, including plain (non-looped) requests between them:

```http
{% for product in Products %}
### Create product {{ forloop.index }}
# @name CreateProduct{{ forloop.index }}
POST {{ApiBaseUrl}}/products
{% endfor %}

### Plain request between the two loops
# @name MidMarker
GET {{ApiBaseUrl}}/health

{% for category in Categories %}
### Create category {{ forloop.index }}
# @name CreateCategory{{ forloop.index }}
POST {{ApiBaseUrl}}/categories
{% endfor %}
```

A `{% for %}` can be nested inside another `{% for %}`. Use `{% assign %}` to capture the outer loop's `forloop.index` under a different name before the inner loop starts, since the inner loop's own `forloop` shadows the outer one:

```http
{% for company in Companies %}
{% assign companyIndex = forloop.index %}
{% for license in company.Licenses %}
### Create license {{ companyIndex }}.{{ forloop.index }}: {{ company.Name }} / {{ license }}
# @name CreateLicense{{ companyIndex }}_{{ forloop.index }}
## TEST-EXPECT-STATUS: [201]
POST {{ApiBaseUrl}}/companies/{{ company.Name }}/licenses
Content-Type: application/json

{ "type": "{{ license }}" }
{% endfor %}
{% endfor %}
```

All requests produced by a nesting-root loop (the outer loop and everything nested inside it) count together against the 1000-request expansion limit.

## Conditions and Assignments

Inside a loop body — or anywhere at the top level of the file, even with no loop involved — you can also use `{% if %}` / `{% elsif %}` / `{% else %}` / `{% unless %}` and `{% assign %}`. These are plain Fluid tags with their usual semantics; TeaPie adds no special behavior beyond making its own variables (see [How Expansion Works](#how-expansion-works-and-what-it-leaves-alone)) and `forloop` available to the conditions.

```http
{% for partner in Partners %}
{% if partner.IsPreferred %}
### Create preferred partner {{ forloop.index }}: {{ partner.Name }}
POST {{ApiBaseUrl}}/partners/preferred
{% elsif forloop.last %}
### Create last partner {{ forloop.index }}: {{ partner.Name }}
POST {{ApiBaseUrl}}/partners
{% else %}
### Create partner {{ forloop.index }}: {{ partner.Name }}
POST {{ApiBaseUrl}}/partners
{% endif %}
Content-Type: application/json

{ "name": "{{ partner.Name }}" }
{% endfor %}
```

A few things worth knowing:

- A condition on an **undefined** name (a typo, or a TeaPie variable that was never set) is falsy rather than an error — this is standard Fluid behavior, not a TeaPie-specific guard.
- Variables tagged as `secret` are not visible to templates. In a condition or an `{% assign %}` they behave like an undefined name, even when a lower-priority scope has a non-secret variable with the same name. They also cannot be used as a loop source (see [Guards and Error Handling](#guards-and-error-handling)).
- A dotted TeaPie variable name (e.g. `Temp.FreePartners`) used directly in a condition is evaluated as a single undefined identifier, not resolved as a nested member access — wrap it as `{{ Temp.FreePartners }}` inside the condition if you need its actual value.
- `{% assign %}` is local to the file being expanded. It never sets or overrides a TeaPie variable, so scripts, other test cases and the variables cache never see it. To share a value beyond the file, set it with `tp.SetVariable(...)` in the `-init.csx` script.
- Within the file, `{% assign %}` follows standard Liquid semantics. A value assigned in a loop body is still set in the following iterations. If the same name was also assigned at the top level before the loop, it keeps the last value after the loop. A `{{ name }}` expression is rendered from an assignment only when that name is assigned at the top level before it, or in the same or an enclosing loop.

## Guards and Error Handling

Templating fails loudly instead of silently producing zero or empty requests:

| Situation | Result |
| --- | --- |
| Collection variable does not exist | Error naming the missing variable and the file* |
| Variable exists but is not a collection (a string counts as not a collection, it is never iterated character by character) | Error stating the variable must be a collection* |
| Variable is tagged as `secret` | Error — its values would otherwise appear in the expanded file, in logs and in `--dump-expanded` output* |
| Collection resolves to zero items (empty list, `()`, or a numeric range with no items) | Error — an accidentally empty collection is almost always a mistake* (for an **inner** loop, a log message instead — see [below](#empty-inner-collections)) |
| A filter in a loop source (e.g. `{% for x in Items \| sort %}`), an unknown or duplicated loop option | Error naming the source and listing the supported [loop options](#loop-options) |
| Loop would expand to more than **1000** requests | Error, to prevent runaway expansion |
| A numeric range bound (e.g. `(1..99999999999)`) is too large to fit a 32-bit integer | Error naming the offending bound, instead of a raw overflow failure* |
| Missing `{% endfor %}`, a stray `{% endfor %}` with no matching `{% for %}`, or malformed `{% for %}` syntax | Error identifying the malformed tag (a file whose only tag is a stray `{% endfor %}` is not a template, so it gets the warning below instead) |
| A file contains `{%` but no supported tag (see [How It Fits the Pipeline](#how-it-fits-the-pipeline)) | Warning (not an error); the file is sent unchanged |
| Nested `{% for %}` loops | Supported — see [Multiple Loops in One File](#multiple-loops-in-one-file); a nesting-root loop's combined request count (across all its nesting levels) is what counts against the 1000-request limit above |
| An item property referenced in the loop body does not exist (e.g. `{{ partner.Typo }}`) | Error naming the missing member |
| Rendering the file requires more than **200 000** Fluid evaluation steps (e.g. many `{{ }}`/`{% %}` expressions repeated across items) | Error suggesting the file be checked for repeated expressions or split into smaller loops — this is separate from, and on top of, the 1000-request cap |
| Two or more requests in a templated file share the same `# @name` after expansion | Warning (not an error; in a file without templates it stays an error) — see [Naming Requests Inside a Loop](#naming-requests-inside-a-loop) |
| An inner loop's collection is empty for every outer item, so it generated no requests | Warning (not an error) — see [Empty Inner Collections](#empty-inner-collections) |
| A loop whose body declares requests generated none (e.g. `offset:` past the end, `limit: 0`, or an `{% if %}` that excluded every item) | Warning (not an error), naming the loop and the file. A loop placed inside a request body (e.g. building a JSON array) declares no requests and is not checked |

All errors include the request file's path to make them actionable.

\* These guards apply to a loop's own source expression. A **top-level** source is resolved once, before rendering. An **inner** loop whose source references an ancestor loop's variable (e.g. `company.Licenses` in the [nested-loop example](#multiple-loops-in-one-file) above) is instead checked during rendering, every time that inner loop is about to run, in exactly the same scope the loop itself sees — so TeaPie variables, `{% assign %}` values and enclosing loop variables (including indexers such as `company.Groups[group]`) resolve the same way in the check as in the loop. A missing property or a value that isn't a collection (including `null`) is an error, unconditionally, the same as for a top-level source, and names the offending outer item(s) as a path such as `region[0].country[1]` — the item's position in its source collection, even when the outer loop uses `offset:` or `reversed`. The one exception is an **empty** per-iteration collection — see [Empty Inner Collections](#empty-inner-collections). The 1000-request and malformed-tag guards are unaffected by any of this.

### Empty Inner Collections

An inner loop's source may legitimately be empty for some outer items — a newly created company with no customers yet is a valid case, not a mistake — so it is **not** an error. That outer item simply produces no requests from the inner loop (or renders the loop's `{% else %}` branch, if it has one), while everything else in the file runs normally:

```http
{% for company in Companies %}
### Create company {{ company.Name }}
POST {{ApiBaseUrl}}/companies

{% for customer in company.Customers %}
### Add customer {{ customer.Name }} to {{ company.Name }}
POST {{ApiBaseUrl}}/companies/{{ company.Name }}/customers
{% endfor %}
{% endfor %}
```

So that this never happens silently, TeaPie reports it in the log:

| Situation | Log message |
| --- | --- |
| The inner collection is empty for **some** outer items | **Debug** (visible with `--debug`), one per empty outer item, naming it — e.g. `Loop over 'company.Customers' in 'seed-req.http' produced zero items for company[1], so it generated no requests there.` |
| The inner collection is empty for **every** outer item, so the inner loop generated no requests at all | **Warning** (visible by default), once per loop — e.g. `Loop over 'company.Customers' in 'seed-req.http' produced zero items for every one of its 2 outer item(s), so it generated no requests at all. Check that the collection is filled as expected.` |

The emptiness is checked against the source collection, before any `limit:`/`offset:` is applied.

## Inspecting the Expanded Content

The expanded requests file only ever exists in memory. To see exactly what is about to run, use the `--dump-expanded` or `--dump-expanded-dir` option of `teapie test`:

```bash
teapie test ./Tests --dump-expanded                     # print each expanded file to the console
teapie test ./Tests --dump-expanded-dir ../expanded     # write each expanded file into ../expanded, keeping its relative path
```

The dump directory must be **outside** the tested collection. A dumped `-req.http` file inside the collection could overwrite the original template, or be picked up as an extra test case by the next run. TeaPie therefore stops with an error instead of writing it there.

Only files that contain templates are dumped, after their `-init.csx` script has run, so the output matches what is actually executed. The requests are still sent as usual — the dump is a by-product of a normal run (expansion depends on variables set by scripts and earlier requests, so it can't be produced without running them). A `.tp` file is written with an extra `.http` extension (e.g. `seed.tp.http`), since only its request part is expanded.

The expansion is also logged:

- **Debug level** — logs the content length before and after expansion (e.g. `120 -> 340`), so you can tell at a glance whether a loop actually expanded anything.
- **Trace level** — logs the **entire** expanded `.http` content for the file, exactly as it is about to be split into individual requests.

Trace is the most detailed logging level and is off by default. Enable it with:

```bash
teapie test -v                                              # verbose console output (includes Trace)
teapie test --log-file debug.log --log-file-log-level Trace # write Trace-level logs, incl. expanded content, to a file
```

See [Logging](logging.md) for all available logging levels and options.

## Reporting

Each request produced by a loop is reported exactly like any other request; there is no separate "loop" concept in the JUnit report. The example below is the report for a test case `Create-Partners-req.http` with:

- a 3-item loop (`{% for partner in Partners %}`), each iteration named `CreatePartner{{ forloop.index }}` and asserting `## TEST-EXPECT-STATUS: [201]`;
- a post-response script that checks each iteration's own response through `tp.Responses["CreatePartner1"]`, `tp.Responses["CreatePartner2"]`, and so on.

```xml
<testsuites name="Create-Partners" tests="5" skipped="0" failures="0" time="0.054" timestamp="...">
  <testsuite name="create-partners" tests="5" skipped="0" failures="0" time="0.054">
    <testcase name="[1] Status code should match one of these: [201]" ... />
    <testcase name="[2] Status code should match one of these: [201]" ... />
    <testcase name="[3] Status code should match one of these: [201]" ... />
    <testcase name="All three loop-generated requests should return 201 (Created)." ... />
    <testcase name="Each loop iteration should have sent its own partner's data (not a copy of one)." ... />
  </testsuite>
</testsuites>
```

All requests from the loop (and any plain requests in the same file) land in the **same** `<testsuite>`, since a test suite corresponds to one `.http`/`.tp` file, not one request. A failing iteration reports as a normal `<failure>` on its own `<testcase>`, with no effect on the other iterations' results.

## Current Limitations

This is a first iteration of templating support. The following are **not** supported yet:

- Any Liquid/Fluid tag besides `{% for %}` / `{% endfor %}` / `{% if %}` / `{% elsif %}` / `{% else %}` / `{% unless %}` / `{% assign %}`
- Templating inside `.csx` scripts (only `.http`/`.tp` request content is expanded)
- External data files (CSV/JSON) as a collection source
- Filters in a loop source (see [Loop Options](#loop-options))
- A top-level loop source other than a TeaPie variable name (dotted names included), an inline literal list or a numeric range — a member path (`Root.Children`), an indexer (`Regions[0].Countries`) or a variable created with `{% assign %}` cannot be looped over at the top level. An inner loop's source is evaluated per outer item only when it starts with an enclosing loop's variable (`company.Licenses`, `company[key]`); any other inner source is resolved like a top-level one
- Parallel execution of the requests produced by a loop
