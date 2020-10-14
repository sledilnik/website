<template>
  <b-table
    responsive
    bordered
    outlined
    hover
    :stickyColumn="' '"
    :sort-desc="true"
    :sticky-header="tableHeight"
    :items="items"
    :fields="fields"
  >
    <template v-slot:head()="scope">
      <div class="text-nowrap">{{ scope.label }}</div>
    </template>
  </b-table>
</template>

<script>
import { mapGetters } from "vuex";

export default {
  props: ["tableHeight"],
  data() {
    return {
      items: [],
      fields: [],
      dimensions: [
        "age.male.todate",
        "age.male.0-4.todate",
        "age.male.5-14.todate",
        "age.male.15-24.todate",
        "age.male.25-34.todate",
        "age.male.35-44.todate",
        "age.male.45-54.todate",
        "age.male.55-64.todate",
        "age.male.65-74.todate",
        "age.male.75-84.todate",
        "age.male.85+.todate"
      ]
    };
  },
  computed: {
    ...mapGetters("tableData", ["tableData", "filterTableData"])
  },
  watch: {
    tableData() {
      this.refreshData();
    }
  },
  methods: {
    refreshData() {
      const { items, fields } = this.filterTableData(this.dimensions);
      this.items = items;
      this.fields = fields;
    }
  }
};
</script>