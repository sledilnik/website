<template>
  <div class="loader-container">
    <img class="logo-part1" src="../assets/svg/covid-animation-1.svg" alt="logo" />
    <img class="logo-part2" src="../assets/svg/covid-animation-2.svg" alt="logo" />
  </div>
</template>

<script>
export default {
  name: 'Loader',
};
</script>

<style scopped lang="sass">

$width: 50px
.loader-container
  margin: 80px auto
  width: $width
  height: $width
  position: relative

.logo-part1,
.logo-part2
  width: 100%
  position: absolute


.logo-part1
  animation: rotate1 3s infinite
  animation-timing-function: linear

  @keyframes rotate1
    0%
      transform: rotate(0deg) scale(1)
    12.5%
      transform: rotate(45deg) scale(1.3)
    25%
      transform: rotate(90deg) scale(1)
    37.5%
      transform: rotate(135deg) scale(1.3)
    50%
      transform: rotate(180deg) scale(1)
    62.5%
      transform: rotate(225deg) scale(1.3)
    75%
      transform: rotate(270deg) scale(1)
    87.5%
      transform: rotate(315deg) scale(1.3)
    100%
      transform: rotate(360deg) scale(1)

.logo-part2
  animation: rotate2 3s infinite
  animation-timing-function: linear

  @keyframes rotate2
    0%
      transform: rotate(0deg) scale(1)
    50%
      transform: rotate(-180deg) scale(1)
    100%
      transform: rotate(-360deg) scale(1)
</style>
