<template>
  <header class="navbar">
    <div class="progress-bar">
      <progress-bar></progress-bar>
    </div>
    <ul class="big-container navbar-content flex is-vertically-centered">
      <li>
        <a href="/">
          <img width="100" src="@/assets/logo.svg" />
        </a>
      </li>
      <li class="is-aligned-right">
        <div class="dropdown">
          <a
            class="login flex is-vertically-centered"
            href="#"
            @click.prevent="profileLoginClick()"
          >
            <img
              class="profile-image"
              :src="
                currentUser.loggedIn
                  ? currentUser.profileImage
                  : '/images/login.png'
              "
              alt
            />
            <span class="login is-hidden-mobile">{{ currentUser.name }}</span>
          </a>
          <user-menu></user-menu>
        </div>
      </li>
    </ul>
    <add-bar v-show="showAddBar" style="display: none"></add-bar>
  </header>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import AddBar from "@/components/AddBar.vue";
import ProgressBar from "@/components/ProgressBar.vue";
import Login from "@/components/Login.vue";
import UserMenu from "@/components/UserMenu.vue";
import User from "@/models/User";
import config from "@/config";

@Component({
  components: {
    AddBar,
    ProgressBar,
    UserMenu
  }
})
export default class extends Vue {
  get currentUser(): User {
    return this.$store.getters.currentUser;
  }

  get showAddBar() {
    return this.$store.getters.showAddBar;
  }

  profileLoginClick() {
    if (this.currentUser.loggedIn) {
      this.$store.dispatch("toggleProfileMenu");
    } else {
      this.$modal.show(
        Login,
        {},
        {
          width: "60%",
          adaptive: true,
          minWidth: 300,
          maxWidth: 500
        }
      );
    }
  }

  async created() {
    // check to see if the user is logged in
    try {
      const user = this.$store.dispatch("getUser");
    } catch (err) {
      // TODO
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">
.dropdown {
  position: relative;
}

.progress-bar {
  height: 6px;
}

.navbar {
  top: 0;
  width: 100%;
  background: white;
  box-shadow: 0px 5px 40px 1px #e8e8e8;
}

.navbar-content {
  li {
    list-style: none;
  }
  a {
    text-decoration: none;
    color: #71777e;
  }
}

.profile-image {
  border-radius: 50%;
  margin-right: 10px;
}

.login {
  margin-left: 5px;
}
</style>
