# Folder `.teapie`

`TeaPie` primarily uses the `.teapie` folder to **cache various artifacts**. However, you can also use it to **store shared functionality**.

When the application is run without a specific `--temp-path`, it searches upward through the directory tree to locate the nearest `.teapie` folder. Once found, its contents are explored — making it an ideal place to store your **environment file** and **initialization script**.

To reference the `.teapie` folder within scripts, use the `$teapie` wildcard. This will be replaced with the actual path during preprocessing, helping to avoid issues with relative paths when files are moved or reorganized.
