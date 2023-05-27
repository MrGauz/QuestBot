import json
import os
import random
import time

import requests


def main():
    """Sends PNG pictures from a specified folder to a Telegram group every 1-3 minutes.
    Requires bots.json, a folder full of photos/memes and a GROUP_ID."""
    GROUP_ID = 0
    png_pictures_path = 'images'
    bots_path = 'bots.json'
    sent_images_path = 'sent_images.json'
    min_delay_seconds = 60
    max_delay_seconds = 180

    # Create sent_images.json if it doesn't exist
    # to avoid sending the same file twice in case of a restart
    if not os.path.isfile(sent_images_path):
        with open(sent_images_path, 'x') as f:
            f.write('[]')

    pictures = []
    for filename in os.listdir('images'):
        if filename.endswith('.png'):
            pictures.append(f'{png_pictures_path}/{filename}')

    with open(bots_path, 'r', encoding='utf-8') as bots_file:
        bots = json.loads(bots_file.read())

    while True:
        for picture in pictures:
            picture = os.path.abspath(picture)

            # Check if the picture has already been sent
            with open(sent_images_path, 'r', encoding='utf-8') as f:
                sent_images = json.loads(f.read())
                if picture in sent_images:
                    continue

            # Choose a random bot to send with
            token = random.choice(bots)['token']

            # Random delay
            delay_seconds = random.randint(min_delay_seconds, max_delay_seconds)
            print(f"Waiting: {delay_seconds} seconds")
            time.sleep(delay_seconds)

            # Send a chat action
            url = f'https://api.telegram.org/bot{token}/sendChatAction'
            data = {'chat_id': GROUP_ID, 'action': 'upload_photo'}
            try:
                requests.post(url, json=data)
            except Exception as e:
                print(f'Could not send chat action: {e}')

            # Send the picture
            url = f'https://api.telegram.org/bot{token}/sendPhoto'
            data = {'chat_id': GROUP_ID}
            try:
                with open(picture, 'rb') as photo:
                    files = {'photo': photo}
                    r = requests.post(url, data=data, files=files)
            except Exception as e:
                print(f'Could not send photo: {e}')
                continue
            if r.status_code != 200:
                print(f'Error sending photo: {r.status_code} {r.text}')
                continue

            print(picture)
            with open(sent_images_path, 'r+', encoding='utf-8') as f:
                sent_images = json.loads(f.read())
                sent_images.append(picture)
                f.seek(0)
                f.truncate()
                f.write(json.dumps(sent_images))


if __name__ == '__main__':
    main()
