const glob = require('glob');
const path = require('path');
const fs = require('fs');

const contentLanguages = ['de', 'en', 'hr', 'sl', 'it']
const localesDir = path.join('src', 'locales')

function extractFile(name) {
    let parts = name.slice(0,-3).split(path.sep);
    let content = fs.readFileSync(name, 'utf8');
    return {
        key: parts[3],
        content: content,
    }
}

function updateLocale(lang, data) {
    const filePath = path.join(localesDir, lang + ".json")
    const json = JSON.parse(fs.readFileSync(filePath, 'utf8'));
    if (!json.content) {
        json.content = {}
    }
    for (var key in data) {
        json.content[key] = data[key]
    }
    fs.writeFileSync(filePath, JSON.stringify(json, null, 4))
}

function extractContentLanguage(lang) {
    glob(path.join('src', 'content', lang, '*.md'), {}, function (err, files) {
        data = {}
        files.forEach((file) => {
            extracted = extractFile(file)
            data[extracted.key] = extracted.content
        })
        updateLocale(lang, data)
    })
}

contentLanguages.forEach(lang => extractContentLanguage(lang))

