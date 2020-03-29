<template>
  <b-table
    responsive
    bordered
    no-border-collapses
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
      <div class="text-nowrap">{{ data.item.date | formatDate('dd. MMMM')  }}</div>
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
          key: 'age.male.todate',
          label: 'Skupno'
        },
        {
          key: 'age.male.0-4.todate',
          label: '0 - 4 leta'
        },
        {
          key: 'age.male.5-14.todate',
          label: '5 - 14 let'
        },
        {
          key: 'age.male.15-24.todate',
          label: '15 - 24 let'
        },
        {
          key: 'age.male.25-34.todate',
          label: '25 - 34 let'
        },
        {
          key: 'age.male.35-44.todate',
          label: '35 - 44 let'
        },
        {
          key: 'age.male.45-54.todate',
          label: '45 - 54 let'
        },
        {
          key: 'age.male.55-64.todate',
          label: '55 - 64 let'
        },
        {
          key: 'age.male.65-74.todate',
          label: '65 - 74 let'
        },
        {
          key: 'age.male.75-84.todate',
          label: '75 - 84 let'
        },
      ]
    };
  }
};
</script>

<style lang="scss">
.b-table-sticky-column {
  width: 120px;
}
</style>