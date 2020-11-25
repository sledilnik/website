<template>
  <div>
    <Time-stamp :date="lastUpdate" />

    <div class="custom-container">
      <div class="static-page-wrapper">
        <h1>Ukrepi in omejitve</h1>
        <p>
          Ob uporabi razpoložljivih virov podatkov smo se trudili kar se da
          celovito zbrati in povzeti trenutno veljavne ukrepe, ki jih je
          sprejela slovenska vlada kot odgovor na pandemijo covid-19, predvsem
          na izbranih področjih, ki se najbolj dotikajo vsakdanjega življenja.
          Informacije, dostopne prek spletnega Sledilnika, vključno s povezavami
          na druge strani, so zbrane iz številnih uradnih virov, s katerimi
          nismo neposredno povezani, zato se je treba zavedati, da so zgolj
          informativne narave in se lahko občasno spreminjajo. Sledilnik zato ne
          zagotavlja točnosti in popolnosti zbranih informacij o ukrepih ter
          izrecno zavrača kakršno koli odgovornost za nadaljnje interpretacije,
          ki naše podatke navajajo kot vir.
        </p>
        <p>
          <a
            href="https://docs.google.com/spreadsheets/u/3/d/e/2PACX-1vRKEPPoL5l7hN6A8hb3tWiD2MGO3Xh6QmGIufEga9FD433HZ3k1iyGNYtZNbMPimg5Z5HF_3BcWx5KK/pubhtml"
            >Zbirna tabela</a
          >
        </p>

        <!-- body -->
          <details
            v-for="item in restrictions"
            :key="item.index"
            :id="`restriction-${item.index}`"
          >
          <summary>
            {{ item.name }}<br>
             <span v-html="item.rule" />
          </summary>
          <div>
            <p><b>Geografska veljavnost:</b> <span v-html="item.geoValidity" /></p>
            <p><b>Veljavnost:</b> <span v-html="item.validity" /></p>
            <p><b>Izjeme:</b><br> <span v-html="item.exceptions" /></p>
            <p><b>Dodatna pravila / tolmačenja:</b> <span v-html="item.extrarule" /></p>
            <p><b>Opombe:</b> <span v-html="item.notes" /></p>
            <p><a :href="item.link" targe="_blank">Povezava do odloka (prečiščeno besedilo)</a></p>
          </div>
          </details>
      </div>
      <!-- <FloatingMenu :list="floatingMenu" title="Ukrepi" /> -->
    </div>
  </div>
</template>

<script>
import { GoogleSpreadsheet } from "@/libs/google-spreadsheet.js";
import TimeStamp from "components/TimeStamp";

window.GoogleSpreadsheet = GoogleSpreadsheet;

export default {
  name: "RestrictionsPage",
  components: {
    TimeStamp,
  },
  data() {
    return {
      floatingMenu: [],
      lastUpdate: null,
      restrictions: [],
    };
  },
  mounted() {
    // note: the spreadsheet must have all cells full, so add "/" to empty cells

    // const url = 'https://spreadsheets.google.com/pub?key=1k2-MOUqiI4qVWIkwx3Bm-1S-t_fLruESh_Shf5rTUYE&hl=en&output=html';
    const url =
      "https://spreadsheets.google.com/pub?key=1NBeTl9d154KHggAAuzjNOrIPriqjBVeU-UIkYo0ZRCY&hl=en&output=html";
    var googleSpreadsheet = new GoogleSpreadsheet();
    googleSpreadsheet.url(url);
    googleSpreadsheet.load((result) => {
      var i, j;

      // TODO get date somewhere
      // date is manually stored in the first cell of the second row
      this.lastUpdate = new Date(
        Date.parse(result["data"][1].replace(/\s/g, ""))
      );

      // the real thing
      // first, iterate the columns, they become chapters
      for (i = 2; i <= 14; i++) {
        this.floatingMenu.push({
          title: result["data"][i],
          link: `restriction-${i}`,
        });

        let restriction = {
          index: i,
          name: result["data"][i],
        };

        // then, iterate rows, they become the data for each chapter
        // first row is (c) so skip
        for (j = 1; j <= 7; j++) {
          // 1 Geografska veljavnost
          // 2 Trenutno pravilo
          // 3 Izjeme
          // 4 Dodatna pravila/ tolmačenja
          // 5 Veljavnost:
          // 6 Opombe in več info:
          // 7 Povezava do odloka (prečiščeno besedilo) oz. novice

          var text = result["data"][j * 14 + i];
          var cat = result["data"][j * 14 + 1]; // header of the row, +1 for the (c) cell
          if (text == "_" || text == "/" || text == null) {
            continue;
          } // skip empty categories

          if (j == 2) {
            //georgrafska veljavnost
            restriction.geoValidity = text;
          } else if (j == 5) {
            // veljavnost (datum)
            restriction.validity = text;
          } else if (j == 3) {
            // izjeme are often lists
            //var list = text.replace(/(\W) (\d+\.)/g, "$1<br />\n$2");
            var list = text.replace(/\n/g, "<br />\n");
            // console.log("izjeme", text, list)
            restriction.exceptions = list;
          } else if (j == 7) {
            // povezave
            // var list = text.replace(/  /g, "\n<br />") + "\n";
            console.log("povezve", text)
            const link = text.replace(/(https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*))/g, "$1");
            console.log("parsed", link)
            restriction.link = link;
          } else if (j == 1) {
            // pravilo
            restriction.rule = text;
          } else if (j == 4) {
            // dodatna pravila
            restriction.extrarule = text;
          } else if (j == 6) {
            // opombe
            var list = text.replace(/  /g, "\n<br />") + "\n";
            var links = list.replace(/(http.*?)[ \n$]/g, "<a href='$1'>$1</a>");
            restriction.notes = links;
          } else {
            console.log("too much data");
          }
        }
        this.restrictions.push(restriction);
      }
    });
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
.custom-container {
  margin: -24px auto 0 auto;
  max-width: 730px;

  @media only screen and (min-width: 768px) {
    margin: 0 auto 65px auto;
    box-shadow: $element-box-shadow;
  }
}

.static-page-wrapper {
  padding: 32px 17px 27px 17px;
  background: #fff;

  @media only screen and (min-width: 768px) {
    padding: 32px 32px 27px 32px;
  }

  li {
    list-style: none;
  }

  h1 {
    font-size: 28px;
    margin-top: 32px;
  }

  h2 {
    font-size: 24px;
    margin-top: 32px;
  }

  h3 {
    font-size: 17px;
    font-style: italic;
    margin-top: 24px;
    margin-bottom: 5px;
  }

  p:not(:last-of-type) {
    margin-bottom: 28px;
  }

  //subtitle
  h1 + p > em {
    display: block;
    font-size: 16px;
    font-style: italic;
    color: rgba(0, 0, 0, 0.8);
    font-weight: 400;
    line-height: 1.7;
    margin-bottom: 48px;

    a {
      font-size: 16px;
    }
  }

  p,
  a,
  span,
  strong {
    font-size: 14px;
    color: $text-c;
    line-height: 1.7;
  }
}

details summary::-webkit-details-marker {
  display: none;
}

.static-page-wrapper,
.footnote {
  a {
    font-weight: 600;
    transition: all 0.35s ease-in-out;
    box-shadow: inset 0 -1px 0 white, inset 0 -4px $yellow;
    text-decoration: none;
    color: rgba(0, 0, 0, 0.8);

    &:hover {
      text-decoration: none;
      color: rgba(0, 0, 0, 0.8);
      font-weight: 600;
      box-shadow: inset 0 -1px 0 white, inset 0 -20px $yellow;
    }

    strong {
      font-weight: 600;
    }
  }
}
</style>
