# Moot Index

Turn research notes into an issue-by-issue index of authorities and pinpoint references for a moot brief.

## Try it

Install the .NET 10 SDK, open this folder, and run:

```sh
dotnet run -- example.tsv
```

Look for `1 supporting, 1 distinguishing` in the output. To use your own data, copy the example and pass the new filename. Use `-` to read from standard input.

## Input

Issue, authority, pinpoint, side (support or distinguish), research note. All example references are fictional classroom materials. Separate columns with actual tabs, omit a header row, and use a period for decimal fractions.

Built for law coursework. [ISCED-F field 042](https://uis.unesco.org/sites/default/files/documents/isced-fields-of-education-and-training-2013-en.pdf).
