import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../../hooks/index';

import { Container, Row, Col, Button, Card, Spinner } from 'react-bootstrap';
import { FaTrash, FaPlus, FaRedo } from 'react-icons/fa';
import api from '../../api';

import { CharacterCreateDto, CharacterDto, CharacterListItemDto } from '../../types/api';
import axios, { AxiosResponse } from 'axios';
import { logout } from '../../store/slices/authSlice';
import SheetCard from './SheetCard';
import { toast } from 'react-toastify';

const SheetSelect: React.FC = () =>
{
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const [characters, setCharacters] = useState<CharacterListItemDto[] | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const fetchCharacters = async () =>
  {
    try
    {
      setIsLoading(true);
      console.log("Getting list of characters...");
      const response: AxiosResponse<CharacterListItemDto[]> = await api.get("/character/characters");
      setCharacters([...response.data]);
      setIsLoading(false);
      if (response.data.length === 0)
      {
        toast.info("No characters found.");
      }
    }
    catch (error)
    {
      setIsLoading(false);
      if (axios.isAxiosError(error))
      {
        if (error.response?.status === 401)
        {
          console.log("Unauthorized");
          dispatch(logout());
          navigate("/login");
          toast.error("Session expired. Please log in.");
        }
        else if (error.response?.status === 404)
        {
          console.log("No characters found.");
          setCharacters([]);
          toast.info("No characters found.");
        }
        else if (error.response?.status === 500)
        {
          toast.error(error.response.data.errors.join(", "));
          setCharacters([]);
        }
        else
        {
          console.error("Error:", error);
          toast.error("Failed to load characters.");
          setCharacters([]);
        }
      }
      else
      {
        console.error("Error:", error);
        toast.error("An unexpected error occurred.");
        setCharacters([]);
      }
    }
  };

  useEffect(() =>
  {
    fetchCharacters();
  }, []);

  const onCharacterAdd = async () =>
  {
    try
    {
      // Placeholder: Replace with form input (e.g., modal with Sheet1 fields)
      const newCharacter: CharacterCreateDto = {
        sheet1: { name: `Character ${characters?.length || 0 + 1}`, level: 1, class: "-" }
      };

      const response = await api.post("/character/characters", newCharacter);
      const createdCharacter: CharacterListItemDto = {
        id: response.data.id, // Adjust based on backend response
        name: newCharacter.sheet1.name,
        level: newCharacter.sheet1.level,
        class: newCharacter.sheet1.class
      };

      setCharacters(prev => (prev ? [...prev, createdCharacter] : [createdCharacter]));
      toast.success("Character added successfully!");
    }
    catch (error)
    {
      if (axios.isAxiosError(error))
      {
        if (error.response?.status === 401)
        {
          dispatch(logout());
          navigate("/login");
          toast.error("Session expired. Please log in.");
        }
        else
        {
          toast.error(error.response?.data.errors.join(", ") || "Failed to add character.");
        }
      }
      else
      {
        toast.error("An unexpected error occurred.");
      }
    }
  };

  const onCharacterDelete = async (cId: number) =>
  {
    try
    {
      await api.delete(`/character/characters/${cId}`);
      setCharacters(prev => (prev ? prev.filter(c => c.id !== cId) : []));
      toast.success("Character deleted successfully!");
    }
    catch (error)
    {
      if (axios.isAxiosError(error))
      {
        if (error.response?.status === 401)
        {
          dispatch(logout());
          navigate("/login");
          toast.error("Session expired. Please log in.");
        }
        else if (error.response?.status === 404)
        {
          toast.warn("Character not found.");
          setCharacters(prev => (prev ? prev.filter(c => c.id !== cId) : [])); // Remove anyway
        }
        else
        {
          toast.error(error.response?.data.errors.join(", ") || "Failed to delete character.");
        }
      }
      else
      {
        toast.error("An unexpected error occurred.");
      }
    }
  };

  const onCharacterClick = async (cId: number) =>
  {
    try
    {
      console.log("Fetching character info...");
      const response: AxiosResponse<CharacterDto> = await api.get(`/character/characters/${cId}`);
    }
    catch (error)
    {
      if (axios.isAxiosError(error))
      {
        if (error.response?.status === 401)
        {
          dispatch(logout());
          navigate("/login");
          toast.error("Session expired. Please log in.");
        }
        else if (error.response?.status === 404)
        {
          toast.warn("Character not found.");
          setCharacters(prev => (prev ? prev.filter(c => c.id !== cId) : [])); // Remove anyway
        }
        else
        {
          toast.error(error.response?.data.errors.join(", ") || "Failed to delete character.");
        }
      }
      else
      {
        toast.error("An unexpected error occurred.");
      }
    }
  }

  const onRefresh = async () =>
  {
    await fetchCharacters();
  };

  return (
    <Container className="mt-5">
      <h2>Выбор персонажа</h2>
      <Row className="row-cols-md-8 d-flex flex-row justify-content-center">
        {characters === null ? (
          <Spinner animation="border" role="status">
            <span className="visually-hidden">Loading...</span>
          </Spinner>
        ) : characters.length === 0 ? (
          <p>No characters found.</p>
        ) : (
          characters.map(c => (
            <SheetCard
              key={c.id}
              character={c}
              onCharacterClick={() => onCharacterClick(c.id)}
              onCharacterDelete={() => onCharacterDelete(c.id)}
            />
          ))
        )}
      </Row>
      <Container className="mt-2 d-flex justify-content-between">
        <FaPlus
          style={{ minWidth: "1.5rem", minHeight: "1.5rem", cursor: "pointer" }}
          onClick={() => onCharacterAdd()}
        />
        <FaRedo
          style={{ minWidth: "1.5rem", minHeight: "1.5rem", cursor: "pointer" }}
          onClick={() => onRefresh()}
        />
      </Container>
    </Container>
  );
}

export default SheetSelect;