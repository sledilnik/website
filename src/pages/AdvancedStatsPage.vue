<template>
  <b-container class="mt-3 stats-page">
    <b-row cols="12">
      <b-col>
        <chart title="Stanje bolnisnicnih postelj" :dataseries="occipiedBedDataseries" />
      </b-col>
    </b-row>
    <b-row cols="12">
      <b-col>
        <chart title="Stanje intenzivnih enot" :dataseries="occipiedIcuDataseries" />
      </b-col>
    </b-row>
    <b-row cols="12">
      <b-col>
        <chart title="Kapacitete" :dataseries="capsDataseries" type="column" />
      </b-col>
    </b-row>
  </b-container>
</template>

<script>
import { mapGetters } from "vuex";
import Chart from "@/components/charts/Chart"

export default {
  name: "AdvancedStatsPage",
  components: {
    Chart
  },
  data() {
    return {
      hospitals: [
        "",
        "ukclj",
        "ukcmb",
        "ukg",
        "bse",
        "bto",
        "sbbr",
        "sbce",
        "sbiz",
        "sbje",
        "sbms",
        "sbng",
        "sbnm",
        "sbpt",
        "sbsg",
        "sbtr"
      ]
    };
  },
  computed: {
    ...mapGetters("hospitals", {
      hospitalName: "hospitalName"
    }),
    occipiedBedDataseries() {
      return this.hospitals.map(id => {
        let name = this.hospitalName(id)
        return {
          name: name ? `Zasedene postelje (${name})` : "Zasedene postelje",
          key: this.seriesKey('hospital', id, 'bed.occupied'),
        }
      })
    },
    occipiedIcuDataseries() {
      return this.hospitals.map(id => {
        let name = this.hospitalName(id)
        return {
          name: name ? `Zasedene ICU enote (${name})` : "Zasedene ICU enote",
          key: this.seriesKey('hospital', id, 'icu.occupied'),
        }
      })
    },
    capsDataseries() {
      return this.hospitals.map(id => {
        let name = this.hospitalName(id)
        return {
          name: name ? `Zmogljivost ICU enot (${name})` : "Zmogljivost ICU enot",
          key: this.seriesKey('hospital', id, 'icu.total'),
        }
      })
    },
  },
  methods: {
    seriesKey(prefix, hospitalId, suffix) {
      if (hospitalId != "") {
        return `${prefix}.${hospitalId}.${suffix}`;
      } else {
        return `${prefix}.${suffix}`;
      }
    },
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="sass">
</style>