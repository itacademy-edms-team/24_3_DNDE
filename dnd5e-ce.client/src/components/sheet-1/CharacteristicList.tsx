import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import CharacteristicCardSVG from './assets/CharacteristicCard.png';
import { Fragment, useMemo } from 'react';
import { AbilityCardCardDataMapType, AbilityCardCardDataType, AbilityCardPropsType, AbilityType } from '../../types/state';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { RootState } from '../../types/state';

import { updateName, updateClass, updateLevel, updateRace, updateBackstory, updateWorldview, updatePlayerName, updateExperience, updateStrength, updateDexterity, updateConstitution, updateIntelligence, updateWisdom, updateCharisma } from '../../store/sheet1Slice';

const AbilityCard: React.FC<AbilityCardPropsType> = ({
  skillState,
  skillHandler,
  cardData
}) => {

  const computedBonus = useMemo(() => {
    return Math.floor((skillState - 10) / 2);
  }, [skillState]);

  return (
    <Card className="p-0 m-0 border-0">
      <Card.Img src={CharacteristicCardSVG} alt="Card Image" />
      <Card.ImgOverlay className="p-0 m-0 d-flex flex-column justify-content-center text-center text-truncate">
        <Form.Label
          htmlFor={ cardData.baseId }
          className="text-uppercase fw-bold p-0 m-0"
        >
          {cardData.label}
        </Form.Label>
        <Form.Control
          value={computedBonus}
          readOnly
          className="p-0 m-0 flex-grow-1 bg-transparent border-0 shadow-none text-center"
        />
        <Form.Control
          type="number"
          className="p-0 m-0 bg-transparent border-0 shadow-none text-center"
          value={ skillState }
          onChange={ skillHandler }
        />
      </Card.ImgOverlay>
    </Card>
  );
}


const CharacteristicList: React.FC = () => {
  const getLabelByAbility = (name: AbilityType) => {
    const labels: Record<AbilityType, string> = {
      strength: "Сила",
      dexterity: "Ловкость",
      constitution: "Телосложение",
      intelligence: "Интеллект",
      wisdom: "Мудрость",
      charisma: "Харизма"
    };
    return labels[name];
  }

  const getIdByAbility = (name: AbilityType) => {
    const capitalizedName = name.charAt(0).toUpperCase() + name.slice(1);
    return { bonusId: `character${capitalizedName}Bonus`, baseId: `character${capitalizedName}Base` };
  }

  const cardDataMap: AbilityCardCardDataMapType = {
    strength: {
      bonusId: "strengthBonusId",
      baseId: "strengthBaseId",
      label: "Сила"
    },
    dexterity: {
      bonusId: "dexterityBonusId",
      baseId: "dexterityBaseId",
      label: "Ловкость"
    },
    constitution: {
      bonusId: "constitutionBonusId",
      baseId: "constitutionBaseId",
      label: "Телосложение"
    },
    intelligence: {
      bonusId: "intelligenceBonusId",
      baseId: "intelligenceBaseId",
      label: "Интеллект"
    },
    wisdom: {
      bonusId: "wisdomBonusId",
      baseId: "wisdomBaseId",
      label: "Мудрость"
    },
    charisma: {
      bonusId: "charismaBonusId",
      baseId: "charismaBaseId",
      label: "Харизма"
    }
  };

  const dispatch = useAppDispatch();

  const strengthBase = useAppSelector((state: RootState) => state.sheet1.abilities.strength.base);
  const dexterityBase = useAppSelector((state: RootState) => state.sheet1.abilities.dexterity.base);
  const constitutionBase = useAppSelector((state: RootState) => state.sheet1.abilities.constitution.base);
  const intelligenceBase = useAppSelector((state: RootState) => state.sheet1.abilities.intelligence.base);
  const wisdomBase = useAppSelector((state: RootState) => state.sheet1.abilities.wisdom.base);
  const charismaBase = useAppSelector((state: RootState) => state.sheet1.abilities.charisma.base);

  const handleStrengthBaseChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateStrength(newValue));
    }
  };

  const handleDexterityBaseChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateDexterity(newValue));
    }
  };

  const handleConstitutionBaseChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateConstitution(newValue));
    }
  };

  const handleIntelligenceBaseChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateIntelligence(newValue));
    }
  };

  const handleWisdomBaseChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateWisdom(newValue));
    }
  };

  const handleCharismaBaseChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateCharisma(newValue));
    }
  };

  return (
    <Container className="border-0 d-flex flex-column justify-content-center align-items-center gap-3">
      <AbilityCard
        skillState={strengthBase}
        skillHandler={handleStrengthBaseChange}
        cardData={cardDataMap.strength}
      />
      <AbilityCard
        skillState={dexterityBase}
        skillHandler={handleDexterityBaseChange}
        cardData={cardDataMap.dexterity}
      />
      <AbilityCard
        skillState={constitutionBase}
        skillHandler={handleConstitutionBaseChange}
        cardData={cardDataMap.constitution}
      />
      <AbilityCard
        skillState={intelligenceBase}
        skillHandler={handleIntelligenceBaseChange}
        cardData={cardDataMap.intelligence}
      />
      <AbilityCard
        skillState={wisdomBase}
        skillHandler={handleWisdomBaseChange}
        cardData={cardDataMap.wisdom}
      />
      <AbilityCard
        skillState={charismaBase}
        skillHandler={handleCharismaBaseChange}
        cardData={cardDataMap.charisma}
      />
    </Container>
  );
}

export default CharacteristicList;