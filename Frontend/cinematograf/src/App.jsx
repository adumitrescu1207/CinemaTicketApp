import 'bootstrap/dist/css/bootstrap.min.css';

import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "./pages/Home";
import Header from "./pages/Header";
import Footer from "./pages/Footer";
import Login from './pages/Login';
import Register from './pages/Register';
import BookTicket from './pages/BookTicket';
import MyAccount from './pages/MyAccount';

function App() {
  return (
    <Router>
      <div
        className="d-flex flex-column min-vh-100"
        style={{ minHeight: "100vh" }}
      >
        <Header />
        <main className="flex-fill">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/book" element={<BookTicket />} />
            <Route path="myaccount" element={<MyAccount />} />
          </Routes>
        </main>
        <Footer />
      </div>
    </Router>
  );
}

export default App;