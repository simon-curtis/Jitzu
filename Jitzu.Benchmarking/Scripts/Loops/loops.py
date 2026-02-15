import sys

# get CLI args or default
arg0 = sys.argv[1] if len(sys.argv) > 1 else "10_000"
limit = int(arg0)

i = 0
while i < limit:
    print(i)
    i += 1