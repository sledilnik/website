const fs = require('fs');

const sectionRegex = /^##(.*)/ms
const questionRegex = /<summary id=([^>]+)>([^<]+)/ms
const answerRegex = /<\/summary>([^<>]+)<\/details>/ms

function camelize(str) {
    return str.toLowerCase().replace("č", "c").replace("ž", "z").replace("š", "s").replace(/(?:^\w|[A-Z]|\b\w)/g, function(word, index) {
      return index === 0 ? word.toLowerCase() : word.toUpperCase();
    }).replace(/\s+/g, '');
  }

function extract(filename) {
    const content = fs.readFileSync(filename, 'utf-8').split('<details>')

    var currentSection = null;
    const locales = {}
    const faq = {}

    for (var i = 0; i < content.length; i++) {
        line = content[i]

        var id, question, answer = null

        section = line.match(sectionRegex)
        if (section) {
            currentSection = camelize(section[1].trim())
            faq[currentSection] = []
            // locales[currentSection] = {
            //     title: currentSection,
            //     qa: {}
            // }
        }

        q = line.match(questionRegex)
        if (q) {
            id = q[1].trim()
            question = q[2].trim()
        }

        ans = line.match(answerRegex)
        if (ans) {
            answer = ans[1].trim()
        }
        
        if (locales[id]) {
            throw new Error(`Duplicated ${id}`)
        }
        locales[id] = { question, answer }
        faq[currentSection].push(id)
    }

    console.log(JSON.stringify(locales, null, 4))
    // console.log(JSON.stringify(faq, null, 4))
}

extract('src/content/en/faq.md')