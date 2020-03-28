<template>
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
    :fields="fields"
  >
    <template v-slot:head()="scope">
      <div class="text-nowrap">{{ scope.label }}</div>
    </template>
    <template v-slot:cell(date)="data">
      <div class="text-nowrap">{{ data.item.dateLocal }}</div>
    </template>
  </b-table>
</template>

<script>
export default {
  props: ["csvdata"],
  data() {
    return {
      fields: [
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
        }
      ]
    };
  }
};
</script>

<style>
</style>