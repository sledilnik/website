<template>
  <details :id="`${id}`">
    <summary>{{ title }}</summary>  
    <p v-html-md="body" />
  </details>
</template>
<script>
export default {
  name: "Collapsable",
  props: {
    id: String,
    title: String,
    body: String,
  },
};
</script>
<style scoped lang="scss">

details {
  margin-bottom: 24px;

  & + details {
    border-top: 1px solid #dedede;
    padding-top: 24px;
  }

  summary {
    cursor: pointer;
    user-select: none;
    font-weight: bold;
    font-stretch: normal;
    font-style: normal;
    line-height: 1.71;
    color: rgba(0, 0, 0, 0.75);
    position: relative;
    padding-right: 10%;

    summary::-webkit-details-marker {
        display: none;
    }

    &:after {
      content: url("../assets/svg/expand-dd.svg");
      display: block;
      position: absolute;
      right: 0;
      top: 0;
    }

    &:focus {
      outline: none;
    }
  }
}

details > *:not(summary) {
  position: relative;
  display: none;
  width: 90%;
}

details > *:nth-child(2) {
  margin-top: 2px;
  padding-top: 12px;
}

details[open] {
  & > *:not(summary) {
    display: block;
    animation: show-dd 0.5s ease-out;
  }

  summary {
    &:after {
      content: url("../assets/svg/close-dd.svg");
    }
  }
}

</style>