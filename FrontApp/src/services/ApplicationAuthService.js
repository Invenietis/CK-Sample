import { AuthService, AuthServiceConfiguration, IAuthServiceConfiguration } from '@signature/webfrontauth'
import axios, { AxiosResponse } from 'axios'

class ApplicationAuthService {
    constructor() {
        this.instance =
            new AuthService(
                new AuthServiceConfiguration({
                    hostname: 'localhost',
                    port: 4324,
                    disableSsl: true
                })
            )
    }
}
export default new ApplicationAuthService();