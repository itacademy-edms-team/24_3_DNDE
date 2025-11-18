import Keycloak from "keycloak-js";

const keycloak = new Keycloak({
 url: "http://127.0.0.1:16000",
 realm: "FinanceTrack",
 clientId: "FinanceTrack-react",
});

export default keycloak;