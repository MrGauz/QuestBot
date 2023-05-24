import json
import random
import time

import requests

GROUP_ID = 0


def main():
    with open('text_messages.json', 'r', encoding='utf-8') as messages_file:
        messages = json.loads(messages_file.read())

    with open('bots.json', 'r', encoding='utf-8') as bots_file:
        bots = json.loads(bots_file.read())

    while True:
        for message in messages:
            token = random.choice(bots)['token']

            # Calculate a realistic typing delay
            typing_delay = message.count(' ') * 1.5
            print(f"Waiting: {typing_delay} seconds")

            # Send chat actions every 5 seconds to keep "typing..."
            while typing_delay > 0:
                url = f'https://api.telegram.org/bot{token}/sendChatAction'
                data = {'chat_id': GROUP_ID, 'action': 'typing'}
                try:
                    r = requests.post(url, json=data)
                except:
                    print('Internet error!')

                time.sleep(min(5, abs(typing_delay)))
                typing_delay -= 5

            # Send the message
            url = f'https://api.telegram.org/bot{token}/sendMessage'
            data = {'chat_id': GROUP_ID, 'text': message}
            try:
                r = requests.post(url, json=data)
            except:
                print('Internet error!')
            print(message)


if __name__ == '__main__':
    main()
