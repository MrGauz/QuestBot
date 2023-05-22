#!/bin/bash

# Check if the argument was passed and if the directory exists
if [ $# -eq 0 ] || [ ! -d "$1" ]; then
    echo "Usage: $0 folder/with/images"
    exit 1
fi

# Go through each file in the directory
for file in "$1"/*; do
    # Only process non-jpg files
    if [ "${file##*.}" != "jpg" ]; then
        base="${file%.*}"
        # If there's no corresponding .jpg file
        if [ ! -e "${base}.jpg" ]; then
            # Convert the file to .jpg format
            echo "Converting $file to jpg..."
            convert "$file" "${base}.jpg"
        fi
    fi
done
