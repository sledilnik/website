<template>
    <div class="collapsable">
        <img
            :class="'icon ' + toolipClass"
            @click="copy"
            v-b-tooltip.top
            :title="tooltipTitle"
            :alt="tooltipTitle"
        />
        <details :id="id">
            <summary>{{ title }}</summary>
            <p v-html-md="body" />
        </details>
    </div>
</template>
<script>
export default {
    name: 'Collapsable',
    props: {
        id: String,
        title: String,
        body: String,
    },
    data() {
        return {
            tooltipTitle: this.$t('embedMaker.copy'),
            toolipClass: 'copy',
        }
    },
    methods: {
        copy($event) {
            const element = $event.target
            const dummy = document.createElement('input')
            let text = window.location.href + '#' + element.nextSibling.id
            if (window.location.hash !== '') {
                text =
                    window.location.href.split('#')[0] +
                    '#' +
                    element.nextSibling.id
            }
            document.body.appendChild(dummy)
            dummy.value = text
            dummy.select()
            document.execCommand('copy')
            document.body.removeChild(dummy)
            this.tooltipTitle = this.$t('embedMaker.copied')
            this.toolipClass = 'check'
            setTimeout(() => {
                this.tooltipTitle = this.$t('embedMaker.copy')
                this.toolipClass = 'copy'
            }, 2000)
        },
    },
}
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
        padding-right: 18%;

        @media only screen and (min-width: 768px) {
            padding-right: 10%;
        }

        summary::-webkit-details-marker {
            display: none;
        }

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
.collapsable {
    position: relative;
}
.icon {
    width: 28px;
    cursor: pointer;
    padding: 5px;
    position: absolute;
    top: -3px;
    right: 30px;
    z-index: 10;
    &.copy {
        content: url('../assets/svg/copy.svg');
    }
    &.check {
        content: url('../assets/svg/check.svg');
    }
}
</style>
