import time
import math
from multiprocessing import Pool, cpu_count

def is_prime(n):
    """
    A deliberately inefficient function to check if a number is prime.
    This is designed to be CPU-intensive.
    """
    if n < 2:
        return False
    if n == 2:
        return True
    if n % 2 == 0:
        return False
    # We check up to the square root of n for divisors
    sqrt_n = int(math.sqrt(n))
    for i in range(3, sqrt_n + 1, 2):
        if n % i == 0:
            return False
    return True

def run_parallel_prime_check(number_range):
    """
    Uses a pool of processes to check for prime numbers in parallel.
    """
    # Use all available CPU cores
    num_cores = cpu_count()
    print(f"🚀 Starting HEAVY prime number search on {num_cores} CPU cores...")
    print(f"Checking numbers from {number_range.start:,} to {number_range.stop:,}")

    start_time = time.time()

    # Create a pool of worker processes
    with Pool(processes=num_cores) as pool:
        # The pool.map function distributes the numbers across the available processes
        # and applies the is_prime function to each one.
        results = pool.map(is_prime, number_range)

    # The result is a list of booleans (True if prime, False if not)
    # We can count the number of primes found.
    prime_count = sum(results)
    end_time = time.time()

    print(f"\n✅ Search complete.")
    print(f"Found {prime_count:,} prime numbers in the specified range.")
    print(f"Total time taken: {end_time - start_time:.2f} seconds.")

if __name__ == "__main__":
    # --- MODIFICATIONS TO MAKE IT HARDER ---
    # 1. We start from a much higher number (checking larger numbers takes longer).
    # 2. The total range of numbers to check is much larger.
    START_NUMBER = 50_000_000
    END_NUMBER = 75_000_000  # This is a range of 25 million numbers
    
    numbers_to_check = range(START_NUMBER, END_NUMBER)

    run_parallel_prime_check(numbers_to_check)