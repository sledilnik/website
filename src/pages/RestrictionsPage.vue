<template>
  <div>
    <Time-stamp :date="lastUpdate" />

    <div class="custom-container">
      <div class="static-page-wrapper">
        <h1>
          {{ $t("restrictionsPage.title") }}
        </h1>
        <div v-html-md="$t('restrictionsPage.description')"></div>

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
            <p><b>{{ $t("restrictionsPage.geoValidity") }}:</b> <span v-html="item.geoValidity" /></p>
            <p><b>{{ $t("restrictionsPage.validity") }}:</b> <span v-html="item.validity" /></p>
            <p><b>{{ $t("restrictionsPage.exceptions") }}:</b><br> <span v-html="item.exceptions" /></p>
            <p><b>{{ $t("restrictionsPage.extrarule") }}:</b> <span v-html="item.extrarule" /></p>
            <p><b>{{ $t("restrictionsPage.notes") }}:</b> <span v-html="item.notes" /></p>
            <p><a :href="item.link" targe="_blank">{{ $t("restrictionsPage.link") }}</a></p>
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
            var list = text.replace(/  /g, "\n<br />") + "\n";
            var links = list.replace(/(http.*?)[ \n$]/g, "<a href='$1'>$1</a>");
            restriction.extrarule = links;
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
<style lang="scss"></style>