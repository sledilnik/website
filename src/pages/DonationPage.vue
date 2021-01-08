<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t("donation.title") }}</h1>
      <div v-html-md="$t('donation.intro')" />

      <h2>{{ $t("donation.monthly.title") }}</h2>
      <div v-html-md="$t('donation.monthly.description')" />

      <stripe-checkout
        ref="checkoutRef"
        mode="subscription"
        :pk="publishableKey"
        :success-url="successURL"
        :cancel-url="cancelURL"
        @loading="v => loading = v"
      />
      <div>
        <span v-for="(item) in stripeSubscriptions" :key="item.price">
          <button @click="submit(item)">{{ $t("donation.monthly.donateButton", {amount: item.amount}) }}</button>
        </span>
      </div>

      <h2>{{ $t("donation.banktransfer.title") }}</h2>
      <img src="../assets/donate-qr.png" class="qr" float="right" />
      <span v-html-md="$t('donation.banktransfer.description')" />
      <table>
        <tr>
          <td>{{ $t("donation.banktransfer.recipient") }}</td>
          <td>Znanstveno društvo Sledilnik<br>Celovška cesta 111<br>Ljubljana<br>Slovenija</td>
        </tr>
        <tr>
          <td>{{ $t("donation.banktransfer.iban") }}</td>
          <td>SI56 6100 0002 5152 059</td>
        </tr>
        <tr>
          <td>{{ $t("donation.banktransfer.purpose") }}</td>
          <td>CHAR</td>
        </tr>
        <tr>
          <td>{{ $t("donation.banktransfer.bankBicSwift") }}</td>
          <td>HDELSI22</td>
        </tr>
        <tr>
          <td>{{ $t("donation.banktransfer.bank") }}</td>
          <td>Delavska hranilnica d.d.<br>Miklošičeva 5<br>Ljubljana<br>Slovenija</td>
        </tr>

      </table>

      <img src="../assets/donate-upn.png" class="upn" />

      <div v-html-md="$t('donation.outro')" />
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
      stripeSubscriptions: [
        {
          amount: 5,
          lineItems:[
            {
              price: 'price_1I6dUEEUtJJGJFIUM4slrZ9g', // The id of the recurring price you created in your Stripe dashboard
              quantity: 1,
            },
          ]
        },
        {
          amount: 20,
          lineItems:[
            {
              price: 'price_1I6dUEEUtJJGJFIURHozzi1V', // The id of the recurring price you created in your Stripe dashboard
              quantity: 1,
            },
          ]
        },
        {
          amount: 50,
          lineItems:[
            {
              price: 'price_1I6dUEEUtJJGJFIUnmagHiRR', // The id of the recurring price you created in your Stripe dashboard
              quantity: 1,
            },
          ]
        },
        {
          amount: 100,
          lineItems:[
            {
              price: 'price_1I6dUFEUtJJGJFIUcWNRbSDC', // The id of the recurring price you created in your Stripe dashboard
              quantity: 1,
            },
          ]
        },
      ],
      successURL: `${location.origin}/${this.$i18n.i18next.language}/donate/thanks`,
      cancelURL: `${location.origin}/${this.$i18n.i18next.language}/donate`,
    };
  },
  methods: {
    // You will be redirected to Stripe's secure checkout page
    submit (item) {
      this.$refs.checkoutRef.lineItems = item.lineItems;
      this.$refs.checkoutRef.redirectToCheckout();
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">

img {
  max-width: 100%;

  &.upn {
    padding: 10px 0px;
  }

  &.qr {
    float: right;
    padding: 0 0 20px 20px, 
  }
}

button {
  padding: 10px;
  margin-right: 20px;
  margin-bottom: 20px;
  background-color: $yellow;
  border: none;
}

</style>
