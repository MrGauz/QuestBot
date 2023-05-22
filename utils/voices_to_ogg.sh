#!/bin/bash

# Check if the argument was passed and if the directory exists
if [ $# -eq 0 ] || [ ! -d "$1" ]; then
    echo "Usage: $0 folder/with/voice_memos"
    exit 1
fi

# Go through each file in the directory
for file in "$1"/*; do
    # Only process non-ogg files
    if [ "${file##*.}" != "ogg" ]; then
        base="${file%.*}"
        # If there's no corresponding .ogg file
        if [ ! -e "${base}.ogg" ]; then
            # Convert the file to .ogg format
	    echo "Converting $file to ogg..."
            ffmpeg -hide_banner -loglevel error -i "$file" -c:a libopus -b:a 192k -ac 1 "${base}.ogg"
        fi
    fi
done
