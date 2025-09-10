# SQLFluff 

## Useful commands

[Link to source](https://docs.sqlfluff.com/en/stable/gettingstarted.html)

```python
sqlfluff lint test.sql --dialect ansi # lints acording to ANSI dialect
sqlfluff fix test.sql --dialect ansi # fix all available lints
sqlfluff fix test.sql --rules <RULE> --dialect ansi # fixes ceartain rules
```