<template>
  <footer>
    <div class="content-wrapper">
      <div class="copy"><span>© 2020 Sledilnik.org</span></div>
      <div class="email">
        <a href="mailto:info@sledilnik.org"><span>info@sledilnik.org</span></a>
      </div>
      <div class="social">
        <a href="http://fb.me/COVID19Sledilnik" target="_blank"
          ><img src="../assets/svg/fb-icon.svg" alt="fb-icon"
        /></a>
        <a href="https://twitter.com/sledilnik" target="_blank"
          ><img src="../assets/svg/tw-icon.svg" alt="tw-icon"
        /></a>
      </div>
    </div>
  </footer>
</template>

<script>
export default {};
</script>

<style scopped lang="sass">

footer
  border-top: 1px solid rgba(0, 0, 0, 0.13)
  background: #fff
  padding: 16px 15px
  min-height: 66px
  display: flex
  align-items: center
  justify-content: center

  @media (min-width: 576px)
    padding: 16px 32px

  .tables &
    position: fixed
    bottom: 0
    left: 0
    right: 0

.content-wrapper
  position: relative
  color: rgba(0, 0, 0, 0.5)
  font-size: 13px
  line-height: 1.3
  padding: 15px 0

  @media (min-width: 576px)
    display: flex
    align-items: center
    padding: 0

  .copy
    margin-bottom: 8px

    @media (min-width: 576px)
      margin-bottom: 0

  .email
    margin-left: auto

  a
    line-height: 1.3
    font-size: 13px
    color: rgba(0, 0, 0, 0.5)
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

    @media (min-width: 576px)
      position: static
      margin-left: 32px
      transform: none

    img
      width: 40px

      @media (min-width: 576px)
        width: 32px

    a + a
      margin-left: 32px
</style>
