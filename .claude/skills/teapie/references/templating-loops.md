# Templating Loops Reference

Expand a single request block in a `.http`/`.tp` file into many independent requests, driven by a collection — for data-driven scenarios (seeding N entities, running the same check against a list of inputs) without copy-pasting request blocks or writing imperative loops in `.csx`.

**Syntax:** `{% for <item> in <source> %}` ... `{% endfor %}`

**Where:** `.http` request files and `.tp` files (in their `--- HTTP` section)

Loop expansion runs **after** the pre-request (`-init.csx`) script and **before** requests are split into individual test steps, so a variable set via `tp.SetVariable(...)` is already available as a loop source. Each resulting request behaves exactly like a normal request: its own name, its own directives, its own report entry. Only a file that contains a supported tag (`{% for %}`, `{% if %}`, `{% unless %}`, `{% assign %}` or `{% raw %}`) is templated. A file using only `{% if %}`/`{% assign %}` with no `{% for %}` is still parsed and rendered (see [Conditions and assignments](#conditions-and-assignments) below). Any other file is left completely unchanged, even if it contains `{%` text (e.g. a `"{%Y-%m-%d}"` body); TeaPie logs a warning for it in case a tag is misspelled. In a templated file, every `{%`, including on `#` comment lines, is Fluid syntax: wrap literal `{%` text in `{% raw %}...{% endraw %}`. A commented-out loop still runs.

## Collection Sources

**1. A variable** (most common — typically set in `-init.csx`):

```http
{% for partner in Partners %}
### Create partner {{ forloop.index }}: {{ partner.Name }}
# @name CreatePartner{{ forloop.index }}
POST {{ApiBaseUrl}}/partners
Content-Type: application/json

{ "name": "{{ partner.Name }}", "registrationId": "{{ partner.RegistrationId }}" }
{% endfor %}
```

Property access is `{{ item.PropertyName }}` and is **case-sensitive**. Dotted variable names work too: `{% for partner in Temp.FreePartners %}`.

**2. An inline literal list:** `{% for status in ("new", "used", "certified") %}` — comma-separated double-quoted strings, booleans (`true`/`false`), or numbers (`1`, `1.5`). An empty list is an error.

**3. A numeric range:** `{% for i in (1..5) %}` — inclusive on both ends (5 iterations).

**Loop options** follow the collection, space-separated, in any order: `limit: n`, `offset: n`, `reversed` (standard Liquid; `n` may be a number or a variable) — e.g. `{% for p in Partners offset: 1 limit: 10 reversed %}`. A loop source does **not** accept filters (`{% for x in Items | sort %}` is an error).

**Top-level sources** must be a TeaPie variable name, an inline list or a range — not a member path (`Root.Children`), an indexer or an `{% assign %}` variable.

## The `forloop` Object

| Field | Meaning |
| --- | --- |
| `forloop.index` | 1-based iteration number |
| `forloop.index0` | 0-based iteration number |
| `forloop.first` | `true` on the first iteration |
| `forloop.last` | `true` on the last iteration |

Usable anywhere in the loop body, including inside `# @name` declarations — expansion happens before request names are parsed.

## Naming Requests Inside a Loop

Always include `{{ forloop.index }}` (or another per-iteration value) in `# @name`, otherwise every iteration shares one name — the last-registered request wins and TeaPie logs a warning (not a failure). In nested loops, combine the outer and inner index (see the example below). Names of requests outside loops must also be unique and must not match any name a loop generates: in a templated file such a collision is only a warning, so it can go unnoticed. Because names differ per iteration, a later request referencing an iteration's response needs `forloop.index` combined with the `prepend`/`append` filters, e.g. `{{ forloop.index | prepend: "Temp.Attachments.CompanyId_" }}`, to build the variable reference dynamically.

## Nested Loops

A `{% for %}` can be nested inside another `{% for %}`, and a file can contain multiple loops (nested or sibling) plus plain requests between them. The inner loop's own `forloop` shadows the outer one — capture the outer index first with `{% assign %}`:

```http
{% for company in Companies %}
{% assign companyIndex = forloop.index %}
{% for license in company.Licenses %}
### Create license {{ companyIndex }}.{{ forloop.index }}: {{ company.Name }} / {{ license }}
# @name CreateLicense{{ companyIndex }}_{{ forloop.index }}
POST {{ApiBaseUrl}}/companies/{{ company.Name }}/licenses
Content-Type: application/json

{ "type": "{{ license }}" }
{% endfor %}
{% endfor %}
```

All requests produced by a nesting-root loop (outer loop + everything nested inside it) count together against the 1000-request expansion limit.

## Conditions and assignments

Inside a loop body — or at the top level with no loop at all — `{% if %}`/`{% elsif %}`/`{% else %}`/`{% unless %}` and `{% assign %}` are also supported, with plain Fluid semantics. An undefined name in a condition is falsy, not an error. A dotted TeaPie variable name (e.g. `Temp.FreePartners`) used directly in a condition is read as member access on `Temp` (undefined), not as the bridged variable — wrap it as `{{ Temp.FreePartners }}` if you need its value. `{% assign %}` is local to the file: it never sets or overrides a TeaPie variable (use `tp.SetVariable(...)` in `-init.csx` to share a value). Inside the file it follows Liquid semantics: a loop-body assignment persists into later iterations, and after the loop for a name also assigned at the top level before it.

## Guards and Error Handling

Templating fails loudly instead of silently producing zero or empty requests:

| Situation | Result |
| --- | --- |
| Collection variable does not exist / isn't a collection / resolves to zero items | Error — naming the file and the problem* |
| Loop source is a variable tagged `secret` | Error — secret variables are never visible to templates (in conditions/`assign` they act as undefined)* |
| A numeric range bound (e.g. `(1..99999999999)`) is too large for a 32-bit integer | Error naming the offending bound, instead of a raw overflow failure* |
| Loop would expand to more than **1000** requests | Error |
| Rendering needs more than **200 000** Fluid evaluation steps (too many `{{ }}`/`{% %}` expressions across items) | Error — separate from, and on top of, the 1000-request cap |
| Missing `{% endfor %}`, a stray `{% endfor %}`, or malformed `{% for %}` syntax | Error identifying the malformed tag |
| A filter in a loop source, or an unknown/duplicated loop option | Error listing the supported loop options |
| An item property referenced in the loop body does not exist (e.g. `{{ partner.Typo }}`) | Error naming the missing member |
| Two or more requests in a templated file share the same `# @name` after expansion | Warning, not an error (last one wins); in a file without templates it stays an error |
| An inner loop's collection is empty for **every** outer item (it generated no requests at all) | Warning, not an error |
| A loop whose body declares requests generated none (`offset:` past the end, `limit: 0`, an `{% if %}` excluding every item) | Warning, not an error |

All of the above errors include the request file's path.

\* For an **inner** loop whose source references an ancestor loop's variable (e.g. `company.Licenses`, `company[key]`), TeaPie checks that source during rendering, each time the inner loop is about to run, in the same scope the loop sees (TeaPie variables, `{% assign %}` values, enclosing loop variables). Missing/`null`/non-collection is still an unconditional error there (naming which outer item by its source position, e.g. `region[0].country[1]`), same as a top-level source — but an **empty** per-iteration collection is not an error (a company with no customers is often valid, not a mistake): that outer item just produces no requests from the inner loop (or renders its `{% else %}` branch). It is never silent, though — each empty outer item gets a **Debug** log message naming it (e.g. `company[1]`), and if the collection is empty for **every** outer item, so the inner loop generated no requests at all, a **Warning** (visible by default) names the loop and the file. If a file must not run with such data, assert it in the `-init.csx`/`-test.csx` script instead.

## Current Limitations

- No Liquid/Fluid tag besides `{% for %}` / `{% endfor %}` / `{% if %}` / `{% elsif %}` / `{% else %}` / `{% unless %}` / `{% assign %}`
- No templating inside `.csx` scripts (only `.http`/`.tp` request content is expanded)
- No external data files (CSV/JSON) as a collection source
- No parallel execution of the requests produced by a loop

To see what a templated file expands to, run `teapie test --dump-expanded` (prints each expanded file to the console) or `teapie test --dump-expanded-dir <directory>` (writes them there, keeping relative paths; the directory must be outside the tested collection). Requests are still sent — the dump is a by-product of a normal run.

For logging/inspection details (Debug/Trace-level expansion logging) and a full JUnit reporting example, see the [Templating Loops documentation](https://www.teapie.fun/docs/templating-loops.html).
