import { useEffect, useState } from "react";
import { Container, Form, Button, Card } from "react-bootstrap";
import { useNavigate } from "react-router-dom";

const BookTicket = () => {
  const navigate = useNavigate();

  const [films, setFilms] = useState([]);
  const [proiectii, setProiectii] = useState([]);
  const [selectedFilm, setSelectedFilm] = useState("");
  const [selectedDate, setSelectedDate] = useState("");
  const [selectedTime, setSelectedTime] = useState("");
  const [selectedSala, setSelectedSala] = useState("");
  const [selectedProiectie, setSelectedProiectie] = useState(null);
  const [locuri, setLocuri] = useState([]);
  const [ocupate, setOcupate] = useState([]);
  const [selectedLoc, setSelectedLoc] = useState(null);
  const [message, setMessage] = useState("");
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("jwt");
    if (!token) return;

    fetch("https://localhost:7278/api/auth/me", {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(res => res.ok ? res.json() : null)
      .then(data => setUser(data))
      .catch(console.error);
  }, []);

  useEffect(() => {
    fetch("https://localhost:7278/api/film")
      .then(res => res.json())
      .then(setFilms)
      .catch(console.error);
  }, []);

  useEffect(() => {
    if (!selectedFilm) return;
    fetch(`https://localhost:7278/api/proiectie/byfilm/${selectedFilm}`)
      .then(res => res.ok ? res.json() : [])
      .then(setProiectii)
      .catch(console.error);
  }, [selectedFilm]);

  useEffect(() => {
    if (!selectedDate || !selectedTime || !selectedSala) return;

    const p = proiectii.find(p =>
      p.dataOraStart.startsWith(selectedDate) &&
      p.dataOraStart.includes(selectedTime) &&
      p.numeSala === selectedSala
    );

    if (p) setSelectedProiectie(p.proiectieId);
    else setSelectedProiectie(null);
  }, [selectedDate, selectedTime, selectedSala, proiectii]);

  useEffect(() => {
    if (!selectedProiectie) return;

    fetch(`https://localhost:7278/api/proiectie/${selectedProiectie}`)
      .then(res => res.json())
      .then(p => {
        fetch(`https://localhost:7278/api/loc/bysala/${p.salaId}`)
          .then(res => res.json())
          .then(setLocuri);
      })
      .catch(console.error);

    fetch(`https://localhost:7278/api/bilet/ocupate/${selectedProiectie}`)
      .then(res => res.ok ? res.json() : [])
      .then(setOcupate)
      .catch(console.error);
  }, [selectedProiectie]);

  const zile = [...new Set(proiectii.map(p => p.dataOraStart.split("T")[0]))];
  const ore = [...new Set(
    proiectii
      .filter(p => p.dataOraStart.startsWith(selectedDate))
      .map(p => p.dataOraStart.split("T")[1].substring(0, 5))
  )];
  const sali = [...new Set(
    proiectii
      .filter(p => p.dataOraStart.startsWith(selectedDate) && p.dataOraStart.includes(selectedTime))
      .map(p => p.numeSala)
  )];

  const handleBook = async () => {
    if (!selectedLoc || !selectedProiectie || !user) {
      setMessage("Selectează toate câmpurile!");
      return;
    }

    setLoading(true);

    try {
      const response = await fetch("https://localhost:7278/api/bilet", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
        body: JSON.stringify({
          proiectieId: selectedProiectie,
          locId: selectedLoc,
          utilizatorId: user.utilizatorId,
          dataRezervare: new Date().toISOString(),
        }),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText);
      }

      const biletRezervat = await response.json();

      alert(`Bilet rezervat cu succes!\nStatus: ${biletRezervat.status}`);

      setSelectedLoc(null);

      fetch(`https://localhost:7278/api/bilet/ocupate/${selectedProiectie}`)
        .then(res => res.ok ? res.json() : [])
        .then(setOcupate);

      navigate("/myaccount");

    } catch (err) {
      console.error(err);
      setMessage("❌ Eroare la rezervare: " + err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container className="py-5">
      <Card className="shadow-lg p-4 mx-auto" style={{ maxWidth: "1400px" }}>
        <h2 className="text-center mb-4">Rezervare bilet</h2>

        <Form.Group className="mb-3">
          <Form.Label>Film</Form.Label>
          <Form.Select
            value={selectedFilm}
            onChange={e => {
              setSelectedFilm(e.target.value);
              setSelectedDate("");
              setSelectedTime("");
              setSelectedSala("");
              setSelectedProiectie(null);
              setSelectedLoc(null);
              setLocuri([]);
            }}
          >
            <option value="">Selectează filmul</option>
            {films.map(f => <option key={f.filmId} value={f.filmId}>{f.titlu}</option>)}
          </Form.Select>
        </Form.Group>

        {selectedFilm && (
          <Form.Group className="mb-3">
            <Form.Label>Zi</Form.Label>
            <Form.Select value={selectedDate} onChange={e => setSelectedDate(e.target.value)}>
              <option value="">Selectează ziua</option>
              {zile.map(z => <option key={z}>{z}</option>)}
            </Form.Select>
          </Form.Group>
        )}

        {selectedDate && (
          <Form.Group className="mb-3">
            <Form.Label>Oră</Form.Label>
            <Form.Select value={selectedTime} onChange={e => setSelectedTime(e.target.value)}>
              <option value="">Selectează ora</option>
              {ore.map(o => <option key={o}>{o}</option>)}
            </Form.Select>
          </Form.Group>
        )}

        {selectedTime && (
          <Form.Group className="mb-3">
            <Form.Label>Sală</Form.Label>
            <Form.Select value={selectedSala} onChange={e => setSelectedSala(e.target.value)}>
              <option value="">Selectează sala</option>
              {sali.map(s => <option key={s}>{s}</option>)}
            </Form.Select>
          </Form.Group>
        )}

        {locuri.length > 0 && (
          <>
            <h5 className="text-center mt-4">Alege locul</h5>
            <div className="d-flex flex-column align-items-center gap-2">
              {Array.from({ length: Math.max(...locuri.map(l => l.numarRand)) }, (_, r) => {
                const randCurent = r + 1;
                const locuriRand = locuri.filter(l => l.numarRand === randCurent);

                return (
                  <div key={randCurent} className="d-flex align-items-center gap-1">
                    <strong style={{ width: 30 }}>{randCurent}</strong>
                    {locuriRand.map(l => {
                      const ocupat = ocupate.includes(l.locId);
                      const esteSelectat = selectedLoc === l.locId;
                      return (
                        <Button
                          key={l.locId}
                          disabled={ocupat}
                          variant={
                            esteSelectat ? "success" : ocupat ? "secondary" : "outline-primary"
                          }
                          onClick={() => setSelectedLoc(l.locId)}
                          style={{ width: 43, height: 43 }}
                        >
                          {l.numarLoc}
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
            <Button
              size="lg"
              variant="danger"
              onClick={handleBook}
              disabled={loading}
            >
              {loading ? "Se rezervă..." : "Confirmă rezervarea"}
            </Button>
          </div>
        )}

        {message && <p className="text-center mt-3 fw-bold">{message}</p>}
      </Card>
    </Container>
  );
};

export default BookTicket;
