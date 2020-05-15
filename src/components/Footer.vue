<template>
  <footer>
    <div class="content-wrapper">
      <div class="copy">
        <span>© 2020 Sledilnik.org</span>
      </div>
      <div class="email">
        <a href="mailto:info@sledilnik.org">
          <span>info@sledilnik.org</span>
        </a>
      </div>
      <div class="social">
        <a href="https://fb.me/COVID19Sledilnik" target="_blank">
          <img src="../assets/svg/fb-icon.svg" alt="Facebook" />
        </a>
        <a href="https://twitter.com/sledilnik" target="_blank">
          <img src="../assets/svg/tw-icon.svg" alt="Twitter" />
        </a>
        <a href="https://medium.com/sledilnik" target="_blank">
          <img src="../assets/svg/medium-icon.svg" alt="Medium" />
        </a>
        <a href="https://github.com/sledilnik" target="_blank">
          <img src="../assets/svg/gh-icon.svg" alt="GitHub" class="gh-icon" />
        </a>
      </div>
    </div>
  </footer>
</template>

<script>
export default {
  name: "Footer"
};
</script>

<style scopped lang="sass">
footer
  border-top: 1px solid rgba(0, 0, 0, 0.13)
  background: #fff
  padding: 0 15px
  min-height: 66px
  display: flex
  align-items: center
  justify-content: center
  margin-top: auto

  @media only screen and (min-width: 768px)
    padding: 16px 32px

.content-wrapper
  position: relative
  color: $text-unselcted
  font-size: 13px
  line-height: 1.3
  padding: 15px 0

  @media only screen and (min-width: 768px)
    display: flex
    align-items: center
    padding: 0

  .copy
    margin-bottom: 8px

    @media only screen and (min-width: 768px)
      margin-bottom: 0
    @media only screen and (max-width: 480px)
      display: inline-block

  .email
    margin-left: auto
    @media only screen and (max-width: 480px)
      display: inline-block
      float: right
      margin-left: 16px
      margin-bottom: 16px

  a
    line-height: 1.3
    font-size: 13px
    color: $text-unselcted
    text-decoration: none

    &:hover
      color: rgba(0, 0, 0, 0.5)

  .social
    position: absolute
    right: 0
    top: 50%
    transform: translateY(-50%)
    // display: block
    // font-size: 20px
    // flex-basis: 100%
    // text-align: right

    @media only screen and (min-width: 768px)
      position: static
      margin-left: 32px
      transform: none

    @media only screen and (max-width: 480px)
      width:100%
      position: static
      transform: none
      display: flex
      justify-content: space-around

    img
      width: 40px

      @media only screen and (min-width: 768px)
        width: 32px

    a + a
      margin-left: 32px

      @media only screen and (max-width: 480px)
        margin-left: 16px

    .gh-icon
      opacity: .25
</style>
