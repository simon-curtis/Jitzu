import random
import sys
import time

def main():
    if len(sys.argv) < 2:
        print("Usage: python benchmark_sort.py <size>")
        return

    size = int(sys.argv[1])

    # Generate a large array of random integers
    numbers = [random.randint(0, 1_000_000) for _ in range(size)]

    # Record start time
    start = time.time()

    # Sort the array
    numbers.sort()

    # Record end time
    end = time.time()

    # Print results in milliseconds
    elapsed_ms = (end - start) * 1000
    print(f"Sorted {size} integers in {elapsed_ms:.2f} ms")

if __name__ == "__main__":
    main()