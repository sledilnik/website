<template>
  <b-tabs pills card>
    <b-tab title="Testiranja in okužbe" active>
      <b-table 
      responsive
      bordered
      outlined
      striped
      hover 
      sort-by="date"
      :sort-desc="true"
      sticky-header="600px"
      :items="csvdata"
      :fields="fieldsTesting">
      <template v-slot:head()="scope">
        <div class="text-nowrap">
          {{ scope.label }}
        </div>
      </template>
      </b-table>
    </b-tab>
    <b-tab title="Okuženi po regijah">
      <b-table 
      responsive
      bordered
      outlined
      striped
      hover 
      sort-by="date"
      :sort-desc="true"
      sticky-header="600px"
      :items="csvdata" 
      :fields="fieldsRegion">
      <template v-slot:head()="scope">
        <div class="text-nowrap">
          {{ scope.label }}
        </div>
      </template>
      </b-table>
    </b-tab>
    <b-tab title="Vse">
      <b-table 
      responsive
      bordered
      outlined
      striped
      hover 
      sort-by="date"
      :sort-desc="true"
      sticky-header="600px"
      :items="csvdata">
      <template v-slot:head()="scope">
        <div class="text-nowrap">
          {{ scope.label }}
        </div>
      </template>
      </b-table>
    </b-tab>
  </b-tabs>
</template>

<script>
import StatsData from "StatsData";

export default {
  data() {
    return {
      csvdata: [],
      fieldsRegion: [
        {
          key: "date",
          headerTitle: "Datum",
          label: "Datum",
          sortable: true,
          stickyColumn: true
        },
        {
          key: "region.lj.todate",
          label: "Ljubljana"
        },
        {
          key: "region.ce.todate",
          label: "Celje"
        },
        {
          key: "region.nm.todate",
          label: "Novo Mesto"
        },
        {
          key: "region.mb.todate",
          label: "Maribor"
        },
        {
          key: "region.kr.todate",
          label: "Kranj"
        },
        {
          key: "region.sg.todate",
          label: "Slovenj Gradec"
        },
        {
          key: "region.po.todate",
          label: "Postojna"
        },
        {
          key: "region.ms.todate",
          label: "Murska Sobota"
        },
        {
          key: "region.kp.todate",
          label: "Koper"
        },
        {
          key: "region.za.todate",
          label: "Zasavje"
        },
        {
          key: "region.kk.todate",
          label: "Krsko"
        },
        {
          key: "region.ng.todate",
          label: "Nova Gorica"
        },
      ],
      fieldsTesting: [
        {
          key: "date",
          headerTitle: "Datum",
          label: "Datum",
          sortable: true,
          stickyColumn: true
        },
        {
          key: 'tests.performed',
          label: 'Opravljenih testov'
        },
        {
          key: 'tests.performed.todate',
          label: 'Opravljenih testov - Skupno'
        },
        {
          key: 'tests.positive',
          label: 'Potrjeno okuženi'
        },
        {
          key: 'tests.positive.todate',
          label: 'Potrjeno okuženi - Skupno'
        },
        {
          key: 'state.in_hospital',
          label: 'Hospitalizirani'
        },
        {
          key: 'state.icu',
          label: 'V intenzivnih negi'
        },
        {
          key: 'state.critical',
          label: 'V kritičnem stanju'
        },
        {
          key: 'state.deceased.todate',
          label: 'Umrli'
        },
        {
          key: 'state.out_of_hospital.todate',
          label: 'Odpuščeni iz bolnišnice'
        },
        {
          key: 'state.recovered.todate',
          label: 'Ozdravljeni'
        },
      ]
    };
  },
  created: async function() {
    let csvdata = await StatsData.data();
    this.csvdata = csvdata
  }
};
</script>

<style>
</style>