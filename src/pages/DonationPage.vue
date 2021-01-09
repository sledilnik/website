<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <h1>{{ $t("donation.title") }}</h1>
      <div v-html-md="$t('donation.intro')" />

      <h2>{{ $t("donation.stripe.monthly.title") }}</h2>
      <div v-html-md="$t('donation.stripe.monthly.description')" />

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
          <button @click="submit(item)">{{ $t("donation.stripe.monthly.donateButton", {amount: item.amount}) }}</button>
        </span>
      </div>

      <h2>{{ $t("donation.banktransfer.title") }}</h2>
      <div v-html-md="$t('donation.banktransfer.description')" />
      <img src="../assets/donate-qr.png" class="qr" />
      <table class="bankDetails">
        <tr>
          <td>{{ $t("donation.banktransfer.recipient") }}</td>
          <td>Znanstveno društvo Sledilnik<br>Celovška cesta 111<br>Ljubljana, Slovenija</td>
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
          <td>Delavska hranilnica d.d.<br>Miklošičeva 5<br>Ljubljana, Slovenija</td>
        </tr>

      </table>

      <img src="../assets/donate-upn.png" class="upn" />

      <div class="outro" v-html-md="$t('donation.outro')" />
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
    this.publishableKey = process.env.VUE_APP_STRIPE_PUBLISHABLE_KEY;
    var items = [];
    var configItems = process.env.VUE_APP_STRIPE_PRODUCTS.split(";");
    configItems.forEach((configItem) => {
      var pair = configItem.split(":")
      items.push({
          amount: pair[0],
          lineItems:[
            {
              price: pair[1],
              quantity: 1,
            },
          ]
        })
      
    });
    return {
      loading: false,
      stripeSubscriptions: items,
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

table.bankDetails {
  font-size: 14px;
  margin: 0;
  width:auto;
  min-width: 350px;

  @media only screen and (max-width: 633px) {
    width: 100%;
    padding: 0;
    min-width: auto;
  }

  td {
    padding: 5px 0;
    width: auto;
  }
}

img {
  max-width: 100%;

  &.upn {
    padding: 10px 0px;
  }

  &.qr {
    float: right;
    padding: 0 0 20px 20px;

    @media only screen and (max-width: 400px) {
      width: 100%;
      padding: 0 0 20px 0;
    }
  }
}

button {
  padding: 10px;
  margin-right: 20px;
  margin-bottom: 20px;
  background-color: $yellow;
  border: none;
}

.outro {
  margin-top:50px;
}

</style>
