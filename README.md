# Moot Index

Import team notes once and print an oral argument sheet grouped by issue, with distinct authorities and pinpoint citations.

## Try it

From this folder with the .NET 10 SDK installed:

```sh
dotnet run -- brief moot.db study.tsv
```

Optional TSV columns: issue, authority, pinpoint, support or distinguish, proposition. Run brief moot.db alone to print the saved sheet. Redirect to a text file to share. One sheet groups each proposition under its issue; print length depends on the case.

The included study.tsv uses tab-separated fields. Comment lines start with `#`.

The database is created on first use. Use a different database filename for another class or team.

The original exercise remains available with `dotnet run -- example.tsv`.

## When to use it

A moot team needs to find its authority and pinpoint for each argument while preparing an oral round.

After importing the sample, run `dotnet run -- brief moot.db --sheet Standing oral.html` and print the issue sheet for a practice round.

Example result:

```text
ORAL ARGUMENT SHEET

Standing — 1 distinct authorities
  Timely filing  [support] Seminar A, p. 7
  Written notice  [support] Seminar A, p. 8
```

Related course or field workflow: [Boston University moot court manual](https://www.bu.edu/law/files/2021/09/Moot-Court-Manual-2021.pdf).
