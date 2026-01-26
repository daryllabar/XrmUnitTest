# Wiki Documentation

This folder contains documentation pages for the XrmUnitTest framework.

## Publishing to GitHub Wiki

The wiki pages in this folder should be synced to the [GitHub Wiki](https://github.com/daryllabar/XrmUnitTest/wiki) for the project.

To update the GitHub Wiki with changes from this folder:

1. Clone the wiki repository:
   ```bash
   git clone https://github.com/daryllabar/XrmUnitTest.wiki.git
   ```

2. Copy the wiki pages:
   ```bash
   cp wiki/*.md XrmUnitTest.wiki/
   ```

3. Commit and push:
   ```bash
   cd XrmUnitTest.wiki
   git add .
   git commit -m "Update wiki documentation"
   git push
   ```

## Wiki Pages

- [Using-PrimaryNameProvider.md](Using-PrimaryNameProvider.md) - Documentation on how to configure and use Primary Name Providers
