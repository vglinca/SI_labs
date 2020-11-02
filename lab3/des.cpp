#include <iostream>
#include <stdint.h>
#include <stdio.h>
#include <bitset>
#include <cstring>
#include <fstream>

#define ROTLEFT_28BIT(x, N) ((((x) << (N)) | ((x) >> (-(N) & 27))) & ((1 << 28) - 1))
#define XTREME_BITS(x) ((x >> 6) & 0x2) | ((x >> 2) & 0x1)
#define MIDDLE_BITS(x) (x >> 3) & 0xF
#define BUFFER_SIZE 2048

using namespace std;

const uint8_t IP[64] = {
    58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4,
    62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8,
    57, 49, 41, 33, 25, 17, 9 , 1, 59, 51, 43, 35, 27, 19, 11, 3,
    61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7,
};

const uint8_t FP[64] = {
    40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31,
    38, 6, 46, 14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29,
    36, 4, 44, 12, 52, 20, 60, 28, 35, 3, 43, 11, 51, 19, 59, 27,
    34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9 , 49, 17, 57, 25,
};

const uint8_t KEY1_P[28] = {
    57, 49, 41, 33, 25, 17, 9 , 1 , 58, 50, 42, 34, 26, 18,
    10, 2 , 59, 51, 43, 35, 27, 19, 11, 3 , 60, 52, 44, 36,
};

const uint8_t KEY2_P[28] = {
    63, 55, 47, 39, 31, 23, 15, 7 , 62, 54, 46, 38, 30, 22,
    14, 6 , 61, 53, 45, 37, 29, 21, 13, 5 , 28, 20, 12, 4 ,
};

const uint8_t COMP_P[48] = {
    14, 17, 11, 24, 1 , 5 , 3 , 28, 15, 6 , 21, 10,
    23, 19, 12, 4 , 26, 8 , 16, 7 , 27, 20, 13, 2 ,
    41, 52, 31, 37, 47, 55, 30, 40, 51, 45, 33, 48,
    44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32,
};

const uint8_t EXT_P[48] = {
    32, 1 , 2 , 3 , 4 , 5 , 4 , 5 , 6 , 7 , 8 , 9 ,
    8 , 9 , 10, 11, 12, 13, 12, 13, 14, 15, 16, 17,
    16, 17, 18, 19, 20, 21, 20, 21, 22, 23, 24, 25,
    24, 25, 26, 27, 28, 29, 28, 29, 30, 31, 32, 1 ,
};

const uint8_t P[32] = {
    16, 7 , 20, 21, 29, 12, 28, 17, 1 , 15, 23, 26, 5 , 18, 31, 10,
    2 , 8 , 24, 14, 32, 27, 3 , 9 , 19, 13, 30, 6 , 22, 11, 4 , 25,
};

const uint8_t S_BOX[8][4][16] = {
    {
        {14, 4 , 13, 1 , 2 , 15, 11, 8 , 3 , 10, 6 , 12, 5 , 9 , 0 , 7 },
        {0 , 15, 7 , 4 , 14, 2 , 13, 1 , 10, 6 , 12, 11, 9 , 5 , 3 , 8 },
        {4 , 1 , 14, 8 , 13, 6 , 2 , 11, 15, 12, 9 , 7 , 3 , 10, 5 , 0 },
        {15, 12, 8 , 2 , 4 , 9 , 1 , 7 , 5 , 11, 3 , 14, 10, 0 , 6 , 13},
    },
    {
        {15, 1 , 8 , 14, 6 , 11, 3 , 4 , 9 , 7 , 2 , 13, 12, 0 , 5 , 10},
        {3 , 13, 4 , 7 , 15, 2 , 8 , 14, 12, 0 , 1 , 10, 6 , 9 , 11, 5 },
        {0 , 14, 7 , 11, 10, 4 , 13, 1 , 5 , 8 , 12, 6 , 9 , 3 , 2 , 15},
        {13, 8 , 10, 1 , 3 , 15, 4 , 2 , 11, 6 , 7 , 12, 0 , 5 , 14, 9 },
    },
    {
        {10, 0 , 9 , 14, 6 , 3 , 15, 5 , 1 , 13, 12, 7 , 11, 4 , 2 , 8 },
        {13, 7 , 0 , 9 , 3 , 4 , 6 , 10, 2 , 8 , 5 , 14, 12, 11, 15, 1 },
        {13, 6 , 4 , 9 , 8 , 15, 3 , 0 , 11, 1 , 2 , 12, 5 , 10, 14, 7 },
        {1 , 10, 13, 0 , 6 , 9 , 8 , 7 , 4 , 15, 14, 3 , 11, 5 , 2 , 12},
    },
    {
        {7 , 13, 14, 3 , 0 , 6 , 9 , 10, 1 , 2 , 8 , 5 , 11, 12, 4 , 15},
        {13, 8 , 11, 5 , 6 , 15, 0 , 3 , 4 , 7 , 2 , 12, 1 , 10, 14, 9 },
        {10, 6 , 9 , 0 , 12, 11, 7 , 13, 15, 1 , 3 , 14, 5 , 2 , 8 , 4 },
        {3 , 15, 0 , 6 , 10, 1 , 13, 8 , 9 , 4 , 5 , 11, 12, 7 , 2 , 14},
    },
    {
        {2 , 12, 4 , 1 , 7 , 10, 11, 6 , 8 , 5 , 3 , 15, 13, 0 , 14, 9 },
        {14, 11, 2 , 12, 4 , 7 , 13, 1 , 5 , 0 , 15, 10, 3 , 9 , 8 , 6 },
        {4 , 2 , 1 , 11, 10, 13, 7 , 8 , 15, 9 , 12, 5 , 6 , 3 , 0 , 14},
        {11, 8 , 12, 7 , 1 , 14, 2 , 13, 6 , 15, 0 , 9 , 10, 4 , 5 , 3 },
    },
    {
        {12, 1 , 10, 15, 9 , 2 , 6 , 8 , 0 , 13, 3 , 4 , 14, 7 , 5 , 11},
        {10, 15, 4 , 2 , 7 , 12, 9 , 5 , 6 , 1 , 13, 14, 0 , 11, 3 , 8 },
        {9 , 14, 15, 5 , 2 , 8 , 12, 3 , 7 , 0 , 4 , 10, 1 , 13, 11, 6 },
        {4 , 3 , 2 , 12, 9 , 5 , 15, 10, 11, 14, 1 , 7 , 6 , 0 , 8 , 13},
    },
    {
        {4 , 11, 2 , 14, 15, 0 , 8 , 13, 3 , 12, 9 , 7 , 5 , 10, 6 , 1 },
        {13, 0 , 11, 7 , 4 , 9 , 1 , 10, 14, 3 , 5 , 12, 2 , 15, 8 , 6 },
        {1 , 4 , 11, 13, 12, 3 , 7 , 14, 10, 15, 6 , 8 , 0 , 5 , 9 , 2 },
        {6 , 11, 13, 8 , 1 , 4 , 10, 7 , 9 , 5 , 0 , 15, 14, 2 , 3 , 12},
    },
    {
        {13, 2 , 8 , 4 , 6 , 15, 11, 1 , 10, 9 , 3 , 14, 5 , 0 , 12, 7 },
        {1 , 15, 13, 8 , 10, 3 , 7 , 4 , 12, 5 , 6 , 11, 0 , 14, 9 , 2 },
        {7 , 11, 4 , 1 , 9 , 12, 14, 2 , 0 , 6 , 10, 13, 15, 3 , 5 , 8 },
        {2 , 1 , 14, 7 , 4 , 10, 8 , 13, 15, 12, 9 , 0 , 3 , 5 , 6 , 11},
    },
};

size_t DEScryption(uint8_t*, uint8_t, uint8_t*, uint8_t*, size_t);
//permutations
uint64_t Initial_Permutation(uint64_t);
uint64_t Final_Permutation(uint64_t);
uint64_t Extension_Permutation(uint32_t);
uint32_t Permutation(uint32_t);
uint64_t Key_Compression_Permutation(uint64_t);
void Substitute_6bits_To_4bits(uint8_t*, uint8_t*);
void Split_And_Permutate_Key_From_56bit_To_28bit(uint64_t, uint32_t*, uint32_t*);
//splits
void Split_48bits_To_6bits(uint64_t, uint8_t*);
void Split_64bits_To_8bits(uint64_t, uint8_t*);
void Split_64bit_To_32bit(uint64_t, uint32_t*, uint32_t*);
//joins
uint32_t Join_4bits_To_32bits(uint8_t*);
uint64_t Join_32bits_To_64bits(uint32_t, uint32_t);
uint64_t Join_28bit_Keys_To_56bit(uint32_t, uint32_t);
//extensions
uint64_t Extend_8bit_to_64bit(uint8_t*);
void Extend_Key(uint64_t, uint64_t*);
void Extend_Key_To_48bit(uint32_t, uint32_t, uint64_t*);
//fesitel
void Feistel_Scheme(uint8_t, uint32_t*, uint32_t*, uint64_t*);
void Feistel_Round(uint32_t*, uint32_t*, uint64_t);
uint32_t Function(uint32_t, uint64_t);
uint32_t RunThroughSbox(uint64_t);
//utils
size_t HandleInput(uint8_t*);
void PrintArr(uint8_t*, size_t, bool);
void Swap(uint32_t*, uint32_t*);
void OutputToFile(uint8_t*, bool);

const char* FILE_NAME = "out.txt";

int main(int argc, char** argv)
{
    uint8_t encrypted_message[BUFFER_SIZE] = { 0 };
    uint8_t decrypted_message[BUFFER_SIZE] = { 0 };
    uint8_t buffer[BUFFER_SIZE] = { 0 };
    uint8_t key8b[8] = { 'D', 'e', 'S', '5', 'e', 'C', 'r', '1' };

    if (argc == 2) {
        for (int i = 0; i < 8; i++) {
            key8b[i] = argv[1][i];
        }
    }

    size_t len = HandleInput(buffer);
    PrintArr(buffer, len, false);

    len = DEScryption(encrypted_message, 'e', key8b, buffer, len);
    PrintArr(encrypted_message, len, true);

    len = DEScryption(decrypted_message, 'd', key8b, encrypted_message, len);
    PrintArr(decrypted_message, len, false);

    return 0;
}

size_t DEScryption(uint8_t* dest, uint8_t cmd, uint8_t* key8b, uint8_t* src, size_t len) {
    
    len = len % 8 == 0 ? len : len + (8 - len % 8);
    
    uint32_t L = 0;
    uint32_t R = 0;
    uint64_t keys48b[16] = { 0 };

    Extend_Key(Extend_8bit_to_64bit(key8b), keys48b);

    for (size_t i = 0; i < len; i += 8) {
        uint64_t newblock64b = Initial_Permutation(Extend_8bit_to_64bit(src + i));
        Split_64bit_To_32bit(newblock64b, &L, &R);
        Feistel_Scheme(cmd, &L, &R, keys48b);
        Split_64bits_To_8bits(Final_Permutation(Join_32bits_To_64bits(L, R)), dest + i);
    }

    return len;
}

void Feistel_Scheme(uint8_t cmd, uint32_t* L, uint32_t* R, uint64_t* keys48b) {
    switch (cmd) {
        case 'e':
            for (int i = 0; i < 16; i++) {
                Feistel_Round(L, R, keys48b[i]);
            }
            Swap(L, R);
            break;
        case 'd':
            for (int i = 15; i >= 0; i--) {
                Feistel_Round(L, R, keys48b[i]);
            }
            Swap(L, R);
            break;
        default:
            cout << "Unrecognized command.";
            break;
    }
}

void Feistel_Round(uint32_t* L, uint32_t* R, uint64_t key48b) {
    uint32_t tmpR = *R;
    *R = Function(*R, key48b) ^ *L;
    *L = tmpR;
}

uint32_t Function(uint32_t block32b, uint64_t key48b) {
    uint64_t block48b = Extension_Permutation(block32b);
    block48b ^= key48b;
    block32b = RunThroughSbox(block48b);

    return Permutation(block32b);
}

uint32_t RunThroughSbox(uint64_t block48b) {
    uint8_t blocks6b[8] = { 0 };
    uint8_t blocks4b[8] = { 0 };

    Split_48bits_To_6bits(block48b, blocks6b);
    Substitute_6bits_To_4bits(blocks6b, blocks4b);

    return Join_4bits_To_32bits(blocks4b);
}

void Substitute_6bits_To_4bits(uint8_t* blocks6b, uint8_t* blocks4b) {
    uint8_t extremeBits, middleBits;
    for (uint8_t i = 0, j = 0; i < 8; i++, j++) {
        extremeBits = XTREME_BITS(blocks6b[i]);
        middleBits = MIDDLE_BITS(blocks6b[i]);
        blocks4b[j] = S_BOX[i][extremeBits][middleBits];
    }
}

void Extend_Key(uint64_t key64b, uint64_t* keys48b) {
    uint32_t L_key, R_key;

    Split_And_Permutate_Key_From_56bit_To_28bit(key64b, &L_key, &R_key);
    Extend_Key_To_48bit(L_key, R_key, keys48b);
}

void Extend_Key_To_48bit(uint32_t block28b_left, uint32_t block_28b_right, uint64_t* keys48b) {
    uint8_t n = 0;
    uint64_t key56b = 0;

    for (uint8_t i = 0; i < 16; i++) {
        n = (i == 0 || i == 1 || i == 8 || i == 15) ? 1 : 2;

        block28b_left = ROTLEFT_28BIT(block28b_left, n);
        block_28b_right = ROTLEFT_28BIT(block_28b_right, n);

        key56b = Join_28bit_Keys_To_56bit(block28b_left, block_28b_right);
        keys48b[i] = Key_Compression_Permutation(key56b);
    }
}

void Split_And_Permutate_Key_From_56bit_To_28bit(uint64_t key56b, uint32_t* left28b_block, uint32_t* right28b_block) {
    for (uint8_t i = 0; i < 28; i++) {
        *left28b_block |= ((key56b >> (64 - KEY1_P[i])) & 0x01) << (31 - i);
        *right28b_block |= ((key56b >> (64 - KEY2_P[i]) & 0x01)) << (31 - i);
    }
}

uint64_t Join_28bit_Keys_To_56bit(uint32_t block28b_1, uint32_t block28b_2) {
    uint64_t key56b = 0;
    key56b = block28b_1 >> 4;
    key56b = ((key56b << 32) | block28b_2) << 4;

    return key56b;
}

uint64_t Key_Compression_Permutation(uint64_t key56b) {
    uint64_t  key48b = 0;

    for (uint8_t i = 0; i < 48; i++) {
        key48b |= ((key56b >> (48 - COMP_P[i])) & 0x01) << (63 - i);
    }

    return key48b;
}

uint32_t Permutation(uint32_t block32b) {
    uint32_t new_block32b = 0;

    for (uint8_t i = 0; i < 32; i++) {
        new_block32b |= ((block32b >> (32 - P[i])) & 0x01) << (31 - i);
    }

    return new_block32b;
}

uint64_t Initial_Permutation(uint64_t src) {
    uint64_t block64b = 0;

    for (uint8_t i = 0; i < 64; i++) {
        block64b |= ((src >> (64 - IP[i])) & 0x01) << (63 - i);
    }

    return block64b;
}

uint64_t Final_Permutation(uint64_t src) {
    uint64_t block64b = 0;
    for (uint8_t i = 0; i < 64; i++) {
        block64b |= ((src >> (64 - FP[i])) & 0x01) << (63 - i);
    }

    return block64b;
}

uint64_t Extension_Permutation(uint32_t block32b) {
    uint64_t block48b = 0;
    for (uint8_t i = 0; i < 48; i++) {
        block48b |= (uint64_t)((block32b >> (32 - EXT_P[i])) & 0x01) << (63 - i);
    }

    return block48b;
}

uint64_t Extend_8bit_to_64bit(uint8_t* block8b) {
    uint64_t block64b = 0;

    for (uint8_t* ptr = block8b; ptr < block8b + 8; ptr++) {
        block64b = (block64b << 8) | *ptr;
    }

    return block64b;
}

uint32_t Join_4bits_To_32bits(uint8_t* blocks4b) {
    uint32_t block32b = 0;

    for (uint8_t* ptr = blocks4b; ptr < blocks4b + 8; ptr++) {
        block32b = (block32b << 4) | *ptr;
    }

    return block32b;
}

uint64_t Join_32bits_To_64bits(uint32_t block32b_1, uint32_t block32b_2) {
    uint64_t block64b = 0;
    block64b = (uint64_t)block32b_1;
    block64b = (uint64_t)(block64b << 32) | block32b_2;

    return block64b;
}

void Split_64bit_To_32bit(uint64_t block64b, uint32_t* block32b_1, uint32_t* block32b_2) {
    *block32b_1 = (uint32_t)(block64b >> 32);
    *block32b_2 = (uint32_t)block64b;
}

void Split_48bits_To_6bits(uint64_t block48b, uint8_t* blocks6b) {
    for (uint8_t i = 0; i < 8; i++) {
        blocks6b[i] = (block48b >> (58 - (i * 6))) << 2;
    }
}

void Split_64bits_To_8bits(uint64_t block64b, uint8_t* blocks8b) {
    for (size_t i = 0; i < 8; ++i) {
        blocks8b[i] = (uint8_t)(block64b >> ((7 - i) * 8));
    }
}

size_t HandleInput(uint8_t* buffer) {

    cout << "Enter a message to encrypt: " << endl;
    size_t pos = 0;
    uint8_t c;
    while ((c = getchar()) != '\n' && pos < BUFFER_SIZE - 1) {
        buffer[pos++] = c;
    }
    cin.clear();
    buffer[pos] = '\0';

    return pos;
}

void PrintArr(uint8_t* arr, size_t len, bool isEncrypted) {
  if(isEncrypted){
  	cout << "Encrypted bytes: " << ends;
    for (int i = 0; i < len; i++) {
        printf("%d ", arr[i]);
    }
    cout << endl;
  }
  else{
    cout << arr << " [";
    for (int i = 0; i < len; i++) {
        printf("%d ", arr[i]);
    }
    cout << "]" << endl;
  }
}

void OutputToFile(uint8_t* arr, bool append) {
    ofstream outfile;
    if (append) {
        outfile.open(FILE_NAME, fstream::app);
    }
    else {
        outfile.open(FILE_NAME);
    }

    outfile << std::dec << arr << endl;
    outfile.close();
}

void Swap(uint32_t* L, uint32_t* R) {
    uint32_t tmp = *L;
    *L = *R;
    *R = tmp;
}
