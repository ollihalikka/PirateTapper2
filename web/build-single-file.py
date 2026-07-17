#!/usr/bin/env python3
"""Build a single self-contained HTML file from web/index.html.

Inlines every asset under web/assets/ (recursively) as a base64 data URI, so
the result is one portable file you can host anywhere or share directly — no
separate asset folder needed.

Usage:
    python3 web/build-single-file.py [output.html]

Default output: web/pirate-tapper-showdown.single.html
Any asset you add under web/assets/ is picked up automatically on the next run.
"""
import base64, json, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(HERE, 'index.html')
ASSETS = os.path.join(HERE, 'assets')
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, 'pirate-tapper-showdown.single.html')

MIME = {'.mp3': 'audio/mpeg', '.jpg': 'image/jpeg', '.png': 'image/png',
        '.json': 'application/json', '.woff2': 'font/woff2'}

# Walk assets/ recursively; key each file by its path relative to assets/
# (forward slashes) so it matches assetUrl(f) at runtime. Non-asset files
# (READMEs, .gitkeep) are skipped.
inline = {}
for root, _dirs, files in os.walk(ASSETS):
    for f in sorted(files):
        ext = os.path.splitext(f)[1].lower()
        if ext not in MIME:
            continue
        full = os.path.join(root, f)
        key = os.path.relpath(full, ASSETS).replace(os.sep, '/')
        data = open(full, 'rb').read()
        inline[key] = f'data:{MIME[ext]};base64,' + base64.b64encode(data).decode()

src = open(SRC).read()

# Strip the standalone document skeleton — an embedding host (or the artifact
# platform) supplies <!doctype>/<html>/<head>/<body>. Keep everything from
# <title> through the scripts.
start = src.index('<title>')
end = src.index('</body>')
content = src[start:end].replace('</head>\n<body>\n', '')

needle = 'const ASSET_INLINE = null;'
assert needle in content, 'ASSET_INLINE marker not found in index.html'
content = content.replace(needle, 'const ASSET_INLINE = ' + json.dumps(inline) + ';')

# Wrap back into a full standalone document for direct hosting.
doc = ('<!doctype html>\n<html lang="en">\n<head>\n<meta charset="utf-8">\n'
       + content
       + '</body>\n</html>\n')

open(OUT, 'w').write(doc)
print(f'wrote {OUT} ({os.path.getsize(OUT)/1e6:.1f} MB, {len(inline)} assets inlined)')
