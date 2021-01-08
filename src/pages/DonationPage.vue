<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t("donation.title") }}</h1>
      <div>
        <stripe-checkout
          ref="checkoutRef"
          mode="subscription"
          :pk="publishableKey"
          :line-items="lineItems"
          :success-url="successURL"
          :cancel-url="cancelURL"
          @loading="v => loading = v"
        />
        <button @click="submit">{{ $t("donation.buttonSubscribe") }}</button>
      </div>
    </div>
  </div>
</template>

<script>
import { StripeCheckout } from '@vue-stripe/vue-stripe';

export default {
  name: 'DonationPage',
  components: {
      StripeCheckout
  },
  data () {
    this.publishableKey = 'pk_test_51I6cVwEUtJJGJFIUgsLpXxbk4DHr2g8Q2ZCIUlzSWbVBtWbcEZJdJuRhyjCxO0xFZdBecREiHx2zirBDPFN1qoeQ00A5Wd3bak' // process.env.STRIPE_PUBLISHABLE_KEY;
    return {
      loading: false,
      lineItems: [
        {
          price: 'price_1I6dUEEUtJJGJFIUeXsfJpCV', // The id of the recurring price you created in your Stripe dashboard
          quantity: 1,
        },
      ],
      successURL: `${location.origin}/${this.$i18n.i18next.language}/donate/thanks`,
      cancelURL: `${location.origin}/${this.$i18n.i18next.language}/donate`,
    };
  },
  methods: {
    // You will be redirected to Stripe's secure checkout page
    submit () {
      this.$refs.checkoutRef.redirectToCheckout();
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss"></style>
