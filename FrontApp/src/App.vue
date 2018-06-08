<template>
  <div id="app">

    <h1>CK-Sample</h1>
    <div v-if="!isAuthenticated()" id="content">
      <button 
      @click="basicLogin()" 
      class="sign-in-button">
        <span>Sign in basicly</span>
      </button>
      <br /><br />

      <span class="mention-text">
        You can <a href="#" @click="remoteLogin()">use GitHub</a> to sign in.
      </span>
    </div>

    <div v-else>
      Welcome to your application, <span class="bold">{{ authInfo.user.name }}</span> ! <br />
      You've signed in with <span class="bold">{{ authInfo.user.schemes[0].name }}</span>.
    </div>
    
  </div>
</template>

<script>
  import ApplicationAuthService from './services/ApplicationAuthService'
  import { AuthServiceConfiguration, ELevel } from '@signature/webfrontauth';

  export default {
    name: 'app',
    data () {
      return {
        providers: {},
        authInfo: null,
        username: null,
        password: null,
        errorMsg: null
      }
    },

    created: function () {
      ApplicationAuthService.instance.addOnChange(() => this.updateInfo());
    },

    mounted: function () {
      this.providers = {
          GitHub: "GitHub"
      };
    },

    methods: {
      updateInfo() {
        this.authInfo = ApplicationAuthService.instance.authenticationInfo;
      },

      isAuthenticated() {
        if (this.authInfo) {
          return (this.authInfo.level >= 2) ? true : false;
        } return false;
      },

      basicLogin() {
        ApplicationAuthService.instance.startPopupBasicLogin();
      },

      remoteLogin() {
        ApplicationAuthService.instance.startPopupLogin(this.providers.GitHub);
      }
    }
  }
</script>

<style>
  #app {
    font-family: 'Avenir', Helvetica, Arial, sans-serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    text-align: center;
    color: #2c3e50;
    margin-top: 60px;
  }

  h1 {
    font-weight: normal;
  }

  a {
    color: #42b983;
    text-decoration: none;
    transition-property: color;
    transition-duration: 0.2s;
  }

  a:hover {
    color: #65daa5;
  }
  
  .bold {
    font-weight: bold;
  }

  .error {
    margin: 10px;
    color: rgb(226, 28, 28);
    font-size: 80%
  }

  .sign-in-button {
    background: linear-gradient(0deg, #42b983, #65daa5);
    width: 8em;
    border: none;
    border-radius: 5px;
    padding: 3px;
    color: white;
    transition-property: box-shadow;
    transition-duration: 0.2s;
  }

  .sign-in-button:hover {
    box-shadow: #c4c4c4 0px 0px 5px;
  }

  .mention-text {
    font-size: 90%
  }
</style>
