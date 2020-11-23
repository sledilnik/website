<template>
  <div>
    <Time-stamp :date="lastUpdate" />
    <div class="custom-container">
      <div class="static-page-wrapper">
        <ul id="restrictionsList">
          <li
            v-for="item in restrictions"
            :key="item.index"
            :id="`restriction-${item.index}`"
          >
            <h2>{{ item.name }}</h2>
            <div><h3>Geografska veljavnost:</h3><span v-html="item.geoValidity" /></div>
            <div><h3>Trenutno pravilo:</h3><span v-html="item.rule" /></div>
            <div><h3>Izjeme:</h3><span v-html="item.exceptions" /></div>
            <div><h3>Veljavnost:</h3><span v-html="item.validity" /></div>
            <div><h3>Povezave</h3><span v-html="item.links" /></div>
          </li>
        </ul>
      </div>
      <FloatingMenu :list="floatingMenu" />
    </div>
  </div>
</template>

<script>
import { GoogleSpreadsheet } from "@/libs/google-spreadsheet.js";
import FloatingMenu from "components/FloatingMenu";
import TimeStamp from "components/TimeStamp";

window.GoogleSpreadsheet = GoogleSpreadsheet;

export default {
  name: "RestrictionsPage",
  components: {
    FloatingMenu,
    TimeStamp,
  },
  data() {
    return {
      floatingMenu: [],
      lastUpdate: new Date(),
      restrictions: [],
    };
  },
  mounted() {
    const url = 'https://spreadsheets.google.com/pub?key=1k2-MOUqiI4qVWIkwx3Bm-1S-t_fLruESh_Shf5rTUYE&hl=en&output=html';
    var googleSpreadsheet = new GoogleSpreadsheet();
    googleSpreadsheet.url(url);
    googleSpreadsheet.load((result) => {
      var i, j;

      // TODO get date somwhere
      this.lastUpdate = new Date(Date.parse(result["data"][0]));

      // the real thing
      for (i = 1; i <= 13; i++) {
        this.floatingMenu.push({
          title: result["data"][i],
          link: `restriction-${i}`,
        });

        let restriction = {
          index: i,
          name: result["data"][i],
        };
        for (j = 1; j <= 7; j++) {
          // 1 Geografska veljavnost
          // 2 Trenutno pravilo
          // 3 Izjeme
          // 4 Dodatna pravila/ tolmačenja
          // 5 Veljavnost:
          // 6 Opombe in več info:
          // 7 Povezava do odloka (prečiščeno besedilo) oz. novice

          var text = result["data"][j * 14 + i];
          if (text == "_" || text == "/" || text == null) {
            continue;
          } // skip empty categories

          if (j == 1) {
            //georgrafska veljavnost
            restriction.geoValidity = text;
          } else if (j == 5) {
            // veljavnost (datum)
            restriction.validity = text;
          } else if (j == 3) {
            // izjeme are often lists

            var list = text.replace(/(\W) (\d+\.)/g, "$1<br />\n$2");
            // console.log("izjeme", text, list)
            restriction.exceptions = list;
          } else if (j == 7) {
            // povezave
            var list = text.replace(/  /g, "\n<br />") + "\n";
            var links = list.replace(/(http.*?)[ \n$]/g, "<a href='$1'>$1</a>");
            restriction.links = links;
          } else {
            restriction.rule = text;
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
