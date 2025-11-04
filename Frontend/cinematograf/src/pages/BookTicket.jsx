import { useEffect, useState } from "react";
import { Container, Form, Button, Card } from "react-bootstrap";

const BookTicket = () => {
  const [films, setFilms] = useState([]);
  const [proiectii, setProiectii] = useState([]);
  const [selectedFilm, setSelectedFilm] = useState("");
  const [selectedProiectie, setSelectedProiectie] = useState(null);
  const [locuri, setLocuri] = useState([]);
  const [ocupate, setOcupate] = useState([]);
  const [selectedLoc, setSelectedLoc] = useState(null);
  const [message, setMessage] = useState("");
  const [user, setUser] = useState(null);

  useEffect(() => {
  const token = localStorage.getItem("jwt");
  if (!token) return;

  fetch("https://localhost:7278/api/auth/me", {
    headers: { "Authorization": `Bearer ${token}` }
  })
    .then(res => res.ok ? res.json() : null)
    .then(data => setUser(data))
    .catch(err => console.error(err));
}, []);

  useEffect(() => {
    fetch("https://localhost:7278/api/film")
      .then(res => res.json())
      .then(data => setFilms(data))
      .catch(err => console.error(err));
  }, []);

  useEffect(() => {
    if (!selectedFilm) return;

    fetch(`https://localhost:7278/api/proiectie/byfilm/${selectedFilm}`)
      .then(res => res.ok ? res.json() : [])
      .then(data => setProiectii(data))
      .catch(err => console.error(err));
  }, [selectedFilm]);

  useEffect(() => {
    if (!selectedProiectie) return;

    fetch(`https://localhost:7278/api/proiectie/${selectedProiectie}`)
      .then(res => res.json())
      .then(data => {
        const salaId = data.sala?.salaId;
        if (!salaId) return;

        fetch(`https://localhost:7278/api/loc/bysala/${salaId}`)
          .then(res => res.json())
          .then(locuriData => setLocuri(locuriData))
          .catch(err => console.error(err));
      })
      .catch(err => console.error(err));

    fetch(`https://localhost:7278/api/bilet/ocupate/${selectedProiectie}`)
      .then(res => res.json())
      .then(data => setOcupate(data))
      .catch(err => console.error(err));
  }, [selectedProiectie]);

  const handleBook = async () => {
  if (!selectedLoc || !selectedProiectie || !user) {
    setMessage("Selectează un loc și o proiecție!");
    return;
  }

  const response = await fetch("https://localhost:7278/api/bilet", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${localStorage.getItem("jwt")}`,
    },
    body: JSON.stringify({
      proiectieId: parseInt(selectedProiectie),
      locId: selectedLoc,
      utilizatorId: user.utilizatorId, // folosim id-ul real
      dataRezervare: new Date().toISOString()
    }),
  });

  if (response.ok) {
    setMessage("✅ Bilet rezervat cu succes!");
  } else {
    const errorText = await response.text();
    console.error("Server error:", errorText);
    setMessage("❌ Eroare la rezervare: " + errorText);
  }
};

  return (
    <Container className="py-5">
      <Card className="shadow-lg p-4 mx-auto" style={{ maxWidth: "800px", borderRadius: "12px" }}>
        <h2 className="text-center mb-4">Rezervare Bilet</h2>

        <Form.Group className="mb-3">
          <Form.Label>Film</Form.Label>
          <Form.Select
            value={selectedFilm}
            onChange={(e) => {
              setSelectedFilm(e.target.value);
              setSelectedProiectie(null);
              setLocuri([]);
              setOcupate([]);
              setSelectedLoc(null);
            }}
          >
            <option value="">Selectează un film</option>
            {films.map(film => (
              <option key={film.filmId} value={film.filmId}>{film.titlu}</option>
            ))}
          </Form.Select>
        </Form.Group>

        {selectedFilm && (
          <Form.Group className="mb-3">
            <Form.Label>Proiecție</Form.Label>
            <Form.Select
              value={selectedProiectie || ""}
              onChange={(e) => setSelectedProiectie(e.target.value)}
            >
              <option value="">Selectează proiecția</option>
              {proiectii.map(p => (
                <option key={p.proiectieId} value={p.proiectieId}>
                  {new Date(p.dataOraStart).toLocaleString()} — Sala {p.sala?.nume || "??"}
                </option>
              ))}
            </Form.Select>
          </Form.Group>
        )}

        {selectedProiectie && locuri.length > 0 && (
          <>
            <h5 className="text-center mb-3">Alege locul:</h5>
            <div className="d-flex flex-column align-items-center gap-2 mb-3">
              {Array.from({ length: Math.max(...locuri.map(l => l.numarRand)) }, (_, r) => {
                const randCurent = r + 1;
                const locuriRand = locuri.filter(l => l.numarRand === randCurent);

                return (
                  <div key={randCurent} className="d-flex align-items-center gap-2">
                    <span style={{ width: "30px", textAlign: "center", fontWeight: "bold" }}>
                      {randCurent}
                    </span>

                    {locuriRand.map(loc => {
                      const ocupat = ocupate.includes(loc.locId);
                      const esteSelectat = selectedLoc === loc.locId;
                      return (
                        <Button
                          key={loc.locId}
                          variant={
                            ocupat
                              ? "secondary"
                              : esteSelectat
                              ? "success"
                              : "outline-primary"
                          }
                          disabled={ocupat}
                          onClick={() => setSelectedLoc(loc.locId)}
                          style={{
                            width: "45px",
                            height: "45px",
                            borderRadius: "8px",
                          }}
                        >
                          {loc.numarLoc}
                        </Button>
                      );
                    })}
                  </div>
                );
              })}
            </div>
          </>
        )}

        {selectedLoc && (
          <div className="text-center mt-4">
            <Button variant="danger" size="lg" onClick={handleBook}>
              Confirmă Rezervarea
            </Button>
          </div>
        )}

        {message && <p className="text-center mt-3 fw-bold text-info">{message}</p>}
      </Card>
    </Container>
  );
};

export default BookTicket;
