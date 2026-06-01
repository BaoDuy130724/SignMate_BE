import re
import unicodedata

def normalize_filename(word):
    return str(word)

sql_path = r'F:\SignMate_AICamera\signmate-ai\dataset\all_updates.sql'
out_path = r'F:\SignMate_BE\SignMate.Infrastructure\Data\SeedData_Signs.sql'

with open(sql_path, 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Convert public."Signs" to [dbo].[Signs]
text = text.replace('public."Signs"', '[dbo].[Signs]')
text = text.replace('"Id"', '[Id]')
text = text.replace('"LessonId"', '[LessonId]')
text = text.replace('"Word"', '[Word]')
text = text.replace('"VideoUrl"', '[VideoUrl]')
text = text.replace('"ThumbnailUrl"', '[ThumbnailUrl]')
text = text.replace('"Description"', '[Description]')
text = text.replace('"OrderIndex"', '[OrderIndex]')
text = text.replace('"ReferenceKeypointData"', '[ReferenceKeypointData]')
text = text.replace('gen_random_uuid()', 'NEWID()')

# 2. Extract and replace the giant JSON string in VALUES (...)
# We look for VALUES ( ... 'word' ... '[[{ ... }]]' )

def replacer(match):
    full_match = match.group(0)
    word = match.group(1)
    filename = f"{normalize_filename(word)}.mp4.json"
    url_str = f"'/dataset/{filename}'"
    # Replace the giant string '[[{...}]]' with url_str
    # The giant string is match.group(2)
    
    # Simple replace:
    res = full_match.replace("'" + match.group(2) + "'", url_str)
    return res

# Pattern explanation:
# VALUES\s*\(\s*NEWID\(\)\s*,\s*'[^']*'\s*,\s*(?:N)?'([^']+)'\s*,.*?'(\[\[\{.*?\}\]\])'\s*\)
# Uses re.DOTALL so .*? can match across lines if needed
new_text = re.sub(
    r"VALUES\s*\(\s*NEWID\(\)\s*,\s*'[^']*'\s*,\s*(?:N)?'([^']+)'\s*,.*?'(\[\[\{.*?\}\]\])'\s*\)",
    replacer,
    text,
    flags=re.DOTALL
)

with open(out_path, 'w', encoding='utf-8') as f:
    f.write(new_text)

print("Done generating updated SeedData_Signs.sql")
