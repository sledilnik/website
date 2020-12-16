<template>
  <div class="custom-container">
    <div class="static-page-wrapper">
      <span v-html="content"></span>
    </div>
  </div>
</template>

<script>
export default {
  name: 'StaticPage',
  props: {
    name: String,
    content: String,
  },
  mounted() {
    this.initDropdowns;

    // open question, if anchor link
    if (this.$route.hash && document.querySelector(this.$route.hash)) {
      document.querySelector(this.$route.hash).parentElement.open=true
    }
  },
  methods: {
    initDropdowns: function() {
      let dropdowns = document.querySelectorAll('.dropdown');
      dropdowns.forEach((dropdown) => {
        dropdown.querySelector('.dd-title').addEventListener('click', () => {
          dropdown.classList.contains('dd-show')
            ? dropdown.classList.remove('dd-show')
            : dropdown.classList.add('dd-show');
        });
      });
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
.custom-container {
  margin: 24px auto 0 auto;
  max-width: 730px;

  @media only screen and (min-width: 768px) {
    margin: 48px auto 65px auto;
    box-shadow: $element-box-shadow;
  }
}

.static-page-wrapper {
  padding: 32px 17px 27px 17px;
  background: #fff;

  @media only screen and (min-width: 768px) {
    padding: 32px 32px 27px 32px;
  }

  h1 {
    margin-bottom: 32px;
  }

  .dropdown + h2,
  .dropdown + h3,
  h1 + h2 {
    margin-top: 64px;
  }

  h2,
  h3,
  h4 {
    margin-bottom: 24px;
  }

  h1 {
    font-size: 28px;
  }

  h2 {
    font-size: 21px;
  }

  h3 {
    font-size: 18px;
  }

  p:not(:last-of-type) {
    margin-bottom: 28px;
  }

  //subtitle
  h1 + p > em {
    display: block;
    font-size: 16px;
    font-style: italic;
    color: rgba(0, 0, 0, 0.8);
    font-weight: 400;
    line-height: 1.7;
    margin-bottom: 48px;

    a {
      font-size: 16px;
    }
  }

  p,
  a,
  span,
  strong {
    font-size: 14px;
    color: $text-c;
    line-height: 1.7;
  }

  * + h1,
  * + h2,
  * + h3,
  * + table {
    margin-top: 48px;
  }

  tr + tr {
    margin-top: 27px;
  }

  table {
    width: 100%;
    table-layout: fixed;
    text-align: left;
  }

  table {
    table-layout: fixed;
    text-align: left;
    td {
      padding: 15px 0;
      width: 50%;
      border-top: 1px solid rgba(0, 0, 0, 0.45);
    }
  }

  //dropdown HTML in MD
  h1 + details,
  h2 + details,
  h3 + details {
    margin-top: 48px;
  }

  details,
  .dropdown {
    margin-bottom: 24px;

    & + details,
    & + .dropdown {
      border-top: 1px solid #dedede;
      padding-top: 24px;
    }

    summary,
    .dd-title {
      cursor: pointer;
      user-select: none;
      font-weight: bold;
      font-stretch: normal;
      font-style: normal;
      line-height: 1.71;
      color: rgba(0, 0, 0, 0.75);
      position: relative;
      padding-right: 10%;

      &:after {
        content: url('../assets/svg/expand-dd.svg');
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

  .dropdown .dd-content {
    display: none;
    margin: 2px 0;
    padding-top: 12px;
    width: 90%;
    position: relative;
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
        content: url('../assets/svg/close-dd.svg');
      }
    }
  }

  .dropdown.dd-show {
    .dd-content {
      display: block;
      animation: show-dd 0.5s ease-out;
    }

    .dd-title {
      &:after {
        content: url('../assets/svg/close-dd.svg');
      }
    }
  }

  @keyframes show-dd {
    from {
      transform: translateY(-8px);
      opacity: 0.1;
    }
    to {
      transform: translateY(0px);
      opacity: 1;
    }
  }

  .img-link {
    display: block;
    box-shadow: none;
    margin-bottom: 24px;

    &:hover {
      box-shadow: none;
    }

    img {
      width: 100%;
    }
  }
}

details summary::-webkit-details-marker {
  display: none;
}

.static-page-wrapper,
.footnote,
.link {
  a {
    font-weight: 600;
    transition: all 0.35s ease-in-out;
    box-shadow: inset 0 -1px 0 white, inset 0 -4px $yellow;
    text-decoration: none;
    color: rgba(0, 0, 0, 0.8);

    &:hover {
      text-decoration: none;
      color: rgba(0, 0, 0, 0.8);
      font-weight: 600;
      box-shadow: inset 0 -1px 0 white, inset 0 -20px $yellow;
    }

    strong {
      font-weight: 600;
    }
  }
}
</style>
