import './style.css'
import { route, initRouter } from './router';
import { renderNavbar } from './components/Navbar';
import { HomePage } from './pages/HomePage';
import { VideoPage } from './pages/VideoPage';
import { LoginPage, RegisterPage } from './pages/LoginPage';
import { UploadPage } from './pages/UploadPage';
import { ProfilePage } from './pages/ProfilePage';

renderNavbar();

route('/', () => HomePage());
route('/video/:id', (p) => VideoPage(p));
route('/login', () => Promise.resolve(LoginPage()));
route('/register', () => Promise.resolve(RegisterPage()));
route('/upload', () => Promise.resolve(UploadPage()));
route('/profile/:id', (p) => ProfilePage(p));

initRouter();