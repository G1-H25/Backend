# SQLFluff 

## Useful commands

[Link to source](https://docs.sqlfluff.com/en/stable/gettingstarted.html)

```python
sqlfluff lint sql/ --config .github/workflows/.sqlfluff # lints from Root repo and configured under .sqlfluff
sqlfluff fix sql/ --config .github/workflows/.sqlfluff # fix all available lints under sql folder
sqlfluff fix sql/ --rules <RULE> --config .github/workflows/.sqlfluff # fixes ceartain rules under sql folder
```