import requests
import concurrent.futures
import time
import threading
import matplotlib.pyplot as plt
import random
import numpy as np
from scipy.stats import linregress

random_words = [
    "whisper", "thunder", "galaxy", "lol", "moonlight", 
    "horizon", "butterfly", "good", "melody", "cascade", 
    "spectrum", "serenity", "radiance", "enigma", "harmony", 
    "rhythm", "luminous", "journey", "echo", "tranquil"
]

response_times = []
success_count = 0
failure_count = 0
status_codes = {}
lock = threading.Lock()

def send_request(server_url, params):
    global success_count, failure_count, status_codes
    try:
        start_time = time.time()
        response = requests.get(server_url, params=params)
        response_time = time.time() - start_time

        with lock:
            response_times.append(response_time)
            if response.status_code == 200:
                success_count += 1
            else:
                failure_count += 1
            status_codes[response.status_code] = status_codes.get(response.status_code, 0) + 1
    except requests.exceptions.RequestException:
        with lock:
            response_times.append(time.time() - start_time)
            failure_count += 1

def continuous_requests(server_url, params, num_requests, interval, total_time):
    start_global = time.time()
    while time.time() - start_global < total_time:
        params["input"] = random.choice(random_words)
        with concurrent.futures.ThreadPoolExecutor(max_workers=num_requests) as executor:
            futures = [
                executor.submit(send_request, server_url, params)
                for _ in range(num_requests)
            ]
            concurrent.futures.wait(futures)
        time.sleep(interval)

def calculate_statistics():
    if not response_times:
        print("Немає даних про час відповіді")
        return

    failure_rate = (failure_count / (success_count + failure_count)) * 100 if (success_count + failure_count) > 0 else 0
    std_deviation = np.std(response_times)
    total_requests = success_count + failure_count
    rps = total_requests / 15

    print(f"Частота відмов: {failure_rate:.2f}%")
    print(f"Рівень стабільності (стандартне відхилення): {std_deviation:.4f} сек")
    print(f"Кількість успішних запитів: {success_count}")
    print(f"Кількість неуспішних запитів: {failure_count}")
    print(f"Кількість запитів на секунду (RPS): {rps:.2f}")
    print("Розподіл статус-кодів:")
    for code, count in status_codes.items():
        print(f"  {code}: {count}")

    time_indices = list(range(len(response_times)))
    slope, intercept, _, _, _ = linregress(time_indices, response_times)
    trend = "збільшується" if slope > 0 else "зменшується" if slope < 0 else "стабільний"
    print(f"Тренд середнього часу відповіді: {trend} (нахил: {slope:.4e})")

def plot_response_times():
    if not response_times:
        print("Немає даних про час відповіді")
        return

    plt.figure(figsize=(10, 6))
    plt.hist(response_times, bins=30, edgecolor='black')
    plt.title('Розподіл часу відповіді серверу')
    plt.xlabel('Час відповіді (секунди)')
    plt.ylabel('Кількість запитів')

    plt.annotate(
        f'Середній час: {np.mean(response_times):.4f} сек\n'
        f'Мінімальний час: {min(response_times):.4f} сек\n'
        f'Максимальний час: {max(response_times):.4f} сек',
        xy=(0.95, 0.95),
        xycoords='axes fraction',
        horizontalalignment='right',
        verticalalignment='top'
    )

    plt.tight_layout()
    plt.show()

def main():
    server_url = "http://localhost:8080"
    params = {"input": random.choice(random_words)}
    num_requests = 1000
    interval = 0.1
    total_time = 10

    request_thread = threading.Thread(
        target=continuous_requests,
        args=(server_url, params, num_requests, interval, total_time),
        daemon=True
    )
    request_thread.start()

    try:
        request_thread.join(timeout=total_time + 1)
        calculate_statistics()
        plot_response_times()
    except KeyboardInterrupt:
        print("\nПрипинення роботи...")

if __name__ == "__main__":
    main()
