import json
import random
import time

import requests


def main():
    """Sends text messages to a Telegram group with a pseudo-random delay.
    Requires text_messages.json, bots.json and a GROUP_ID."""
    GROUP_ID = -1001674866129  # todo
    text_messages_path = 'text_messages.json'
    bots_path = 'bots.json'

    with open(text_messages_path, 'r', encoding='utf-8') as messages_file:
        messages = json.loads(messages_file.read())
        random.shuffle(messages)

    with open(bots_path, 'r', encoding='utf-8') as bots_file:
        bots = json.loads(bots_file.read())

    while True:
        for message in messages:
            # Choose a random bot to send with
            token = random.choice(bots)['token']

            # Calculate a realistic typing delay
            typing_delay = message.count(' ') * 1.5
            print(f"Waiting: {int(typing_delay)} seconds")

            # Send chat actions every 5 seconds to keep "typing..."
            while typing_delay > 0:
                url = f'https://api.telegram.org/bot{token}/sendChatAction'
                data = {'chat_id': GROUP_ID, 'action': 'typing'}
                try:
                    requests.post(url, json=data)
                except Exception as e:
                    print(f'Could not send chat action: {e}')

                time.sleep(min(5, abs(typing_delay)))
                typing_delay -= 5

            # Send the message
            url = f'https://api.telegram.org/bot{token}/sendMessage'
            data = {'chat_id': GROUP_ID, 'text': message}
            try:
                r = requests.post(url, json=data)
            except Exception as e:
                print(f'Could not send text message: {e}')
                continue
            if r.status_code != 200:
                print(f'Error sending text message: {r.status_code} {r.text}')
                continue

            print(message)


if __name__ == '__main__':
    main()
