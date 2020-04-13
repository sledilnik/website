<template>
  <b-table
    responsive
    bordered
    no-border-collapse
    outlined
    hover
    sort-by="date"
    :sort-desc="true"
    :sticky-header="tableHeight"
    :items="items"
    :fields="fields"
  ></b-table>
</template>

<script>
import { mapGetters } from "vuex";
import _ from "lodash";
export default {
  props: ["tableHeight"],
  data() {
    return {
      items: [],
      fields: [],
      dimensions: [
        "region.kr.todate",
        "region.ng.todate",
        "region.nm.todate",
        "region.sg.todate",
        "region.kp.todate",
        "region.lj.todate",
        "region.mb.todate",
        "region.ms.todate",
        "region.kk.todate",
        "region.po.todate",
        "region.ce.todate",
        "region.za.todate",
        "region.todate"
      ]
    };
  },
  computed: {
    ...mapGetters("stats", ["regions"]),
    ...mapGetters("tableData", ["tableData", "filterTableData"])
  },
  watch: {
    tableData() {
      const { items, fields } = this.filterTableData(this.dimensions);
      this.items = items;
      this.fields = fields;
    }
  }
};
</script>

<style>
</style>