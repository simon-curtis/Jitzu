import sys

class OneToTenK:
    """
    Iterator that yields the integers from 1 to a specified limit (default 10,000).
    """

    def __init__(self, limit=10000):
        self.current = 1
        self.limit = limit

    def __iter__(self):
        return self

    def __next__(self):
        if self.current > self.limit:
            raise StopIteration
        value = self.current
        self.current += 1
        return value


# Usage example
if __name__ == "__main__":
    limit = 10000
    if len(sys.argv) > 1:
        try:
            limit = int(sys.argv[1])
        except ValueError:
            print(f"Error: '{sys.argv[1]}' is not a valid integer")
            sys.exit(1)
    
    # Create iterator with specified limit
    iterator = OneToTenK(limit)
    
    # Use proper iteration
    for number in iterator:
        print(number)