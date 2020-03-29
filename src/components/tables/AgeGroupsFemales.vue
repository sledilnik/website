<template>
  <b-table
    responsive
    bordered
    outlined
    no-border-collapse
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
          stickyColumn: true,
          variant: 'grey'
        },
        {
          key: 'age.female.todate',
          label: 'Skupno'
        },
        {
          key: 'age.female.0-4.todate',
          label: '0 - 4 leta'
        },
        {
          key: 'age.female.5-14.todate',
          label: '5 - 14 let'
        },
        {
          key: 'age.female.15-24.todate',
          label: '15 - 24 let'
        },
        {
          key: 'age.female.25-34.todate',
          label: '25 - 34 let'
        },
        {
          key: 'age.female.35-44.todate',
          label: '35 - 44 let'
        },
        {
          key: 'age.female.45-54.todate',
          label: '45 - 54 let'
        },
        {
          key: 'age.female.55-64.todate',
          label: '55 - 64 let'
        },
        {
          key: 'age.female.65-74.todate',
          label: '65 - 74 let'
        },
        {
          key: 'age.female.75-84.todate',
          label: '75 - 84 let'
        },
      ]
    };
  }
};
</script>

<style>
</style>