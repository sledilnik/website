<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t("donation.title") }}</h1>
      <div class="intro" v-html-md="$t('donation.intro')" />

      <h2>{{ $t("donation.monthly.title") }}</h2>
      <div class="intro" v-html-md="$t('donation.monthly.description')" />
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

      <h2>{{ $t("donation.banktransfer.title") }}</h2>
      <div class="intro" v-html-md="$t('donation.banktransfer.description')" />
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
          <td>Delavska hranilnica d.d.<br>Miklošičeva 5<br>Ljubljana<br>Slovenia</td>
        </tr>

      </table>

      <img src="../assets/donate-qr.png" class="qr" />
      <img src="../assets/donate-upn.png" class="upn" />

      <div class="intro" v-html-md="$t('donation.outro')" />
    </div>
  </div>
</template>

<script>
import { StripeCheckout } from '@vue-stripe/vue-stripe';
import AgeGroupsFemales from '../components/tables/AgeGroupsFemales.vue';

console.log(process.env.VUE_APP_CONTENT_ENDPOINT_BASE)
console.log(process.env.VUE_APP_DONATEPK)
console.log(process.env.VUE_APP_LOCALE_CONTEXT)

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
<style lang="scss">

.upn {
  max-width: 100%;
}

</style>
