<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t("donation.title") }}</h1>

      <div v-html-md="$t('donation.thankyou')" />
    </div>
  </div>
</template>

<script>

export default {
  name: 'DonationThanksPage',
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss"></style>
