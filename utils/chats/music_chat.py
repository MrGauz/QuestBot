import json
import os
import random
import time

import requests


def main():
    """Sends MP3 files from a specified folder to a Telegram group every 2-5 minutes.
    Requires bots.json, a folder with music files and a GROUP_ID."""
    GROUP_ID = 0
    music_path = 'music'
    bots_path = 'bots.json'
    sent_music_path = 'sent_music.json'
    min_delay_seconds = 120
    max_delay_seconds = 300

    # Create sent_music.json if it doesn't exist
    # to avoid sending the same music twice in case of a restart
    if not os.path.isfile(sent_music_path):
        with open(sent_music_path, 'x') as f:
            f.write('[]')

    files = []
    for filename in os.listdir(music_path):
        if filename.endswith('.mp3'):
            files.append(f'{music_path}/{filename}')

    with open(bots_path, 'r', encoding='utf-8') as bots_file:
        bots = json.loads(bots_file.read())

    for music in files:
        music = os.path.abspath(music)

        # Check if the music has already been sent
        with open(sent_music_path, 'r', encoding='utf-8') as f:
            sent_files = json.loads(f.read())
            if music in sent_files:
                continue

        # Choose a random bot to send with
        token = random.choice(bots)['token']

        # Random delay
        delay_seconds = random.randint(min_delay_seconds, max_delay_seconds)
        print(f"Waiting: {delay_seconds} seconds")
        time.sleep(delay_seconds)

        # Send a chat action
        url = f'https://api.telegram.org/bot{token}/sendChatAction'
        data = {'chat_id': GROUP_ID, 'action': 'upload_audio'}
        try:
            requests.post(url, json=data)
        except Exception as e:
            print(f'Could not send chat action: {e}')

        # Send the picture
        url = f'https://api.telegram.org/bot{token}/sendDocument'
        data = {'chat_id': GROUP_ID}
        try:
            with open(music, 'rb') as f:
                files = {'document': f}
                r = requests.post(url, data=data, files=files)
        except Exception as e:
            print(f'Could not send music: {e}')
            continue
        if r.status_code != 200:
            print(f'Error sending music: {r.status_code} {r.text}')
            continue

        print(music)
        with open(sent_music_path, 'r+', encoding='utf-8') as f:
            sent_files = json.loads(f.read())
            sent_files.append(music)
            f.seek(0)
            f.truncate()
            f.write(json.dumps(sent_files))


if __name__ == '__main__':
    main()
