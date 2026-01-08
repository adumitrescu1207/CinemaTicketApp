import { useEffect, useState } from "react";
import { Navbar, Nav, Container, Button } from "react-bootstrap";
import { Link } from "react-router-dom";
import logo from "../images/movie-ticket.png";

const Header = () => {
  const [user, setUser] = useState(null);

  useEffect(() => {
    const token = localStorage.getItem("jwt");
    if (!token) return;

    fetch("https://localhost:7278/api/auth/me", {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
      .then(res => res.ok ? res.json() : null)
      .then(data => {
        if (data) setUser(data);
      })
      .catch(err => console.error(err));
  }, []);

  const handleLogout = () => {
    localStorage.removeItem("jwt");
    fetch("https://localhost:7278/api/auth/logout", { method: "POST" });
    setUser(null);
    window.location.href = "/";
  };

  return (
    <Navbar bg="dark" variant="dark" expand="lg" sticky="top">
      <Container>
        <Navbar.Brand as={Link} to="/">
          <img
            src={logo}
            alt="Logo"
            height="80"
            className="me-2"
            style={{ borderRadius: "5px" }}
          />
        </Navbar.Brand>

        <Navbar.Toggle aria-controls="basic-navbar-nav" />

        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="ms-auto align-items-center">
            <Nav.Link as={Link} to="/">Acasă</Nav.Link>

            {user && (
              <Nav.Link as={Link} to="/book">
                <strong>Rezervă bilet</strong>
              </Nav.Link>
            )}

            {user && (
              <Nav.Link as={Link} to="/myaccount">
                <strong>Contul meu</strong>
              </Nav.Link>
            )}

            {user ? (
              <>
                <span className="text-light m-4">
                  Bun venit, <strong>{user.nume}</strong>!
                </span>
                <Button
                  variant="outline-light"
                  size="sm"
                  onClick={handleLogout}
                >
                  Deconectare
                </Button>
              </>
            ) : (
              <Nav.Link as={Link} to="/login">Autentificare</Nav.Link>
            )}
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default Header;
